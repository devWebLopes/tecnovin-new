# 🐳 Skill: DevOps & Docker — .NET 9 + Oracle

## Sobre
Esta skill define o padrão de infraestrutura, containerização e CI/CD para o projeto GestaoNew. Cobre Docker multi-stage builds, docker-compose para desenvolvimento local, health checks e pipeline CI/CD.

## Dockerfile — Empresa.Api (Multi-Stage Build)

```dockerfile
# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia e restaura dependências (cache otimizado)
COPY Empresa.sln ./
COPY Empresa.Api/Empresa.Api.csproj Empresa.Api/
COPY Empresa.Data/Empresa.Data.csproj Empresa.Data/
COPY Empresa.Util/Empresa.Util.csproj Empresa.Util/
COPY Empresa.Worker/Empresa.Worker.csproj Empresa.Worker/
RUN dotnet restore Empresa.Api/Empresa.Api.csproj

# Copia todo o código fonte e compila
COPY . .
WORKDIR /src/Empresa.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Estágio 2: Runtime (imagem mínima)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Cria usuário não-root para segurança
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /app/logs \
    && chown -R appuser:appuser /app

# Copia binários publicados
COPY --from=build /app/publish ./

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/api/health || exit 1

# Usuário não-root
USER appuser

# Porta exposta
EXPOSE 8080

# Entrypoint
ENTRYPOINT ["dotnet", "Empresa.Api.dll"]
```

## Dockerfile — Empresa.Worker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Empresa.sln ./
COPY Empresa.Worker/Empresa.Worker.csproj Empresa.Worker/
COPY Empresa.Data/Empresa.Data.csproj Empresa.Data/
COPY Empresa.Util/Empresa.Util.csproj Empresa.Util/
RUN dotnet restore Empresa.Worker/Empresa.Worker.csproj

COPY . .
WORKDIR /src/Empresa.Worker
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /app/logs \
    && chown -R appuser:appuser /app

COPY --from=build /app/publish ./

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "Empresa.Worker.dll"]
```

## .dockerignore

```
**/bin
**/obj
**/logs
**/.vs
**/.vscode
**/node_modules
**/.git
**/.gitignore
**/*.user
**/appsettings.Development.json
**/secrets.json
Dockerfile
docker-compose.yml
README.md
docs/
agents/
commands/
skills/
```

## docker-compose.yml — Desenvolvimento Local

```yaml
version: '3.8'

services:
  # API .NET 9
  gestaonew-api:
    build:
      context: .
      dockerfile: Dockerfile
      target: runtime
    container_name: gestaonew-api
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - ORACLE_CONNECTION_STRING=User Id=${ORACLE_USER};Password=${ORACLE_PASS};Data Source=oracle:1521/${ORACLE_SERVICE};
      - JWT_SECRET=${JWT_SECRET}
    depends_on:
      oracle:
        condition: service_healthy
    volumes:
      - ./Empresa.Api/logs:/app/logs
    networks:
      - gestaonew-net
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/api/health"]
      interval: 15s
      timeout: 5s
      retries: 3
      start_period: 20s

  # Worker Service
  gestaonew-worker:
    build:
      context: .
      dockerfile: Dockerfile.worker
    container_name: gestaonew-worker
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ORACLE_CONNECTION_STRING=User Id=${ORACLE_USER};Password=${ORACLE_PASS};Data Source=oracle:1521/${ORACLE_SERVICE};
    depends_on:
      gestaonew-api:
        condition: service_healthy
    volumes:
      - ./Empresa.Worker/logs:/app/logs
    networks:
      - gestaonew-net
    restart: unless-stopped

  # Banco Oracle (desenvolvimento local)
  oracle:
    image: container-registry.oracle.com/database/free:latest
    container_name: gestaonew-oracle
    ports:
      - "1521:1521"
    environment:
      - ORACLE_PWD=${ORACLE_PASS}
      - ORACLE_CHARACTERSET=AL32UTF8
    volumes:
      - oracle-data:/opt/oracle/oradata
    networks:
      - gestaonew-net
    healthcheck:
      test: ["CMD", "sqlplus", "-L", "sys/${ORACLE_PASS}@//localhost:1521/FREEPDB1", "as", "sysdba", "SELECT 1 FROM DUAL;"]
      interval: 30s
      timeout: 10s
      retries: 10
      start_period: 60s

  # Frontend React (desenvolvimento)
  gestaonew-web:
    image: node:20-alpine
    container_name: gestaonew-web
    working_dir: /app
    command: sh -c "npm install && npm run dev -- --host 0.0.0.0"
    ports:
      - "5173:5173"
    volumes:
      - ./Empresa.Web:/app
      - /app/node_modules
    environment:
      - VITE_API_URL=http://localhost:5000
    networks:
      - gestaonew-net
    depends_on:
      - gestaonew-api

volumes:
  oracle-data:
    driver: local

networks:
  gestaonew-net:
    driver: bridge
```

## Variáveis de Ambiente (.env)

```bash
# .env — NUNCA commitar este arquivo!
ORACLE_USER=gestaonew
ORACLE_PASS=senha_segura_aqui
ORACLE_SERVICE=FREEPDB1
JWT_SECRET=chave-super-secreta-com-no-minimo-32-caracteres
```

## Pipeline CI/CD — GitHub Actions

```yaml
# .github/workflows/ci.yml
name: CI - Build & Test

on:
  pull_request:
    branches: [main, develop]
  push:
    branches: [develop]

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  build-and-test:
    name: Build & Test (.NET ${{ matrix.dotnet }})
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run unit tests
        run: dotnet test --configuration Release --no-build --verbosity normal

      - name: Run tests with coverage
        run: |
          dotnet test /p:CollectCoverage=true \
            /p:CoverletOutputFormat=cobertura \
            /p:CoverletOutput=./coverage/ \
            --no-build

      - name: Upload coverage report
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: coverage/

  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Run security analysis
        run: |
          dotnet list package --vulnerable
          # Adicionar ferramentas SAST: SonarQube, Snyk, etc.

  docker-build:
    name: Docker Build (Test)
    runs-on: ubuntu-latest
    needs: build-and-test

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Build Docker image
        run: docker build -t gestaonew-api:test -f Dockerfile .

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: gestaonew-api:test
          format: table
          exit-code: 1
          severity: CRITICAL,HIGH
```

## Pipeline CD — Deploy

```yaml
# .github/workflows/cd.yml
name: CD - Deploy

on:
  push:
    branches: [main]

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  deploy:
    name: Deploy to Production
    runs-on: ubuntu-latest
    environment: production

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release --no-restore

      - name: Test
        run: dotnet test -c Release --no-build

      - name: Publish
        run: dotnet publish Empresa.Api/Empresa.Api.csproj -c Release -o ./publish

      - name: Build Docker image
        run: |
          docker build -t gestaonew-api:${{ github.sha }} -f Dockerfile .
          docker tag gestaonew-api:${{ github.sha }} gestaonew-api:latest

      - name: Push to Container Registry
        run: |
          echo "${{ secrets.REGISTRY_PASSWORD }}" | docker login -u "${{ secrets.REGISTRY_USERNAME }}" --password-stdin
          docker push gestaonew-api:${{ github.sha }}
          docker push gestaonew-api:latest

      # Deploy (ajustar conforme infraestrutura: Kubernetes, Docker Swarm, VM)
      - name: Deploy
        run: |
          # Exemplo: deploy via SSH em VM
          ssh ${{ secrets.DEPLOY_HOST }} "
            docker pull gestaonew-api:latest &&
            docker-compose -f /opt/gestaonew/docker-compose.yml up -d gestaonew-api
          "
```

## Health Checks

### Endpoint na API

```csharp
// Em Program.cs
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithTags("Health")
.ExcludeFromDescription() // Não aparece no Swagger
.AllowAnonymous();

// Health check com validação de banco
app.MapGet("/api/health/database", async (DbSession session) =>
{
    try
    {
        await session.Connection.OpenAsync();
        var version = await session.Connection.QueryFirstAsync<string>(
            "SELECT BANNER FROM V$VERSION WHERE ROWNUM = 1");
        return Results.Ok(new
        {
            status = "Healthy",
            database = "Connected",
            oracleVersion = version
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: "Database Unavailable");
    }
})
.WithTags("Health")
.ExcludeFromDescription()
.AllowAnonymous();
```

## Estrutura de Configuração por Ambiente

```
Empresa.Api/
├── appsettings.json              # Configurações padrão (não sensíveis)
├── appsettings.Development.json  # Overrides de desenvolvimento
├── appsettings.Staging.json      # Overrides de homologação
└── appsettings.Production.json   # Overrides de produção

# appsettings.json (valores não sensíveis)
{
  "Logging": {
    "LogLevel": { "Default": "Information" }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "GestaoNew",
    "Audience": "GestaoNew",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "ConnectionStrings": {
    "Oracle": "User Id=;Password=;Data Source=;"  # Será sobrescrito
  }
}

# ⚠️ NUNCA commitar secrets nos appsettings!
# Usar variáveis de ambiente ou User Secrets:
# dotnet user-secrets set "Jwt:Secret" "chave-super-secreta"
# dotnet user-secrets set "ConnectionStrings:Oracle" "..."
```

## Regras Obrigatórias

1. **SEMPRE** usar multi-stage builds no Dockerfile (reduz imagem final)
2. **SEMPRE** rodar container como usuário não-root
3. **SEMPRE** configurar HEALTHCHECK no Dockerfile e docker-compose
4. **SEMPRE** adicionar `.dockerignore` para evitar enviar binários, logs e secrets
5. **NUNCA** commitar `.env`, `appsettings.Development.json` com secrets, ou `secrets.json`
6. **SEMPRE** usar variáveis de ambiente para secrets (nunca hardcoded)
7. **SEMPRE** rodar `dotnet test` no pipeline CI antes de buildar imagem
8. **SEMPRE** escanear imagem Docker por vulnerabilidades (Trivy, Snyk)
9. **SEMPRE** configurar health checks de banco e API
10. **NUNCA** expor porta Oracle diretamente na internet (apenas rede interna Docker)

## Comandos Úteis

```bash
# Build local
docker build -t gestaonew-api:dev -f Dockerfile .

# Subir ambiente completo
docker-compose up -d

# Ver logs
docker-compose logs -f gestaonew-api

# Verificar health
curl http://localhost:5000/api/health

# Parar tudo
docker-compose down

# Limpar volumes (cuidado: apaga dados do Oracle!)
docker-compose down -v