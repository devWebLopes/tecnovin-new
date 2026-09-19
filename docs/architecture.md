# 🏗️ Arquitetura do Projeto — GestãoNew

## Visão Geral da Arquitetura

O projeto **GestãoNew** segue os princípios da **Clean Architecture** (Arquitetura Limpa), organizando o código em camadas concêntricas com dependências direcionadas para o centro (regras de negócio).

### Diagrama de Camadas

```
┌────────────────────────────────────────────────────────────────┐
│                   EMPRESA.API (Apresentação)                    │
│  Endpoints (Minimal API) · Services · DTOs · Middleware        │
│  Depende de: Data, Util                                        │
├────────────────────────────────────────────────────────────────┤
│                  EMPRESA.WORKER (Background)                    │
│  BackgroundService · Jobs Agendados · Processamento Lote       │
│  Depende de: Data, Util                                        │
├────────────────────────────────────────────────────────────────┤
│                   EMPRESA.DATA (Infraestrutura)                 │
│  Models · Repositories · DbSession · Dapper Queries            │
│  Depende de: Util (apenas)                                     │
├────────────────────────────────────────────────────────────────┤
│                   EMPRESA.UTIL (Cross-Cutting)                  │
│  Utilitários · Helpers · Constantes · Email Service            │
│  Depende de: Nenhuma (camada mais interna)                     │
└────────────────────────────────────────────────────────────────┘
```

## Princípios da Clean Architecture Aplicados

### 1. Independência de Frameworks
- O .NET é o framework, mas as regras de negócio estão nas camadas internas (Data e Util)
- Trocar de framework não impacta as entidades de domínio

### 2. Testabilidade
- As camadas são desacopladas por interfaces
- É possível testar Services sem depender do banco (mocking de repositórios)

### 3. Independência de UI
- A API pode ser substituída por outro mecanismo de apresentação (gRPC, GraphQL)
- O Worker pode ser substituído por outro executor de background (Azure Functions, Hangfire)

### 4. Independência de Banco de Dados
- Dapper + Oracle podem ser substituídos por outro ORM/banco
- A camada Data isola completamente a tecnologia de acesso

### 5. Independência de Agente Externo
- O serviço de e-mail (Empresa.Util.Email) é abstraído por interface
- Pode ser trocado sem afetar outras camadas

## Regras de Dependência (Rigorosas)

| Camada | Pode Referenciar | Proibido Referenciar |
|--------|-----------------|---------------------|
| `Empresa.Api` | `Empresa.Data`, `Empresa.Util` | `Empresa.Worker` |
| `Empresa.Worker` | `Empresa.Data`, `Empresa.Util` | `Empresa.Api` |
| `Empresa.Data` | `Empresa.Util` (apenas) | `Empresa.Api`, `Empresa.Worker` |
| `Empresa.Util` | Nenhuma | Qualquer outra camada |

### Violações Comuns (Proibidas)
```csharp
// ❌ VIOLAÇÃO: Data referenciando Api
using Empresa.Api.Services;

// ❌ VIOLAÇÃO: Api referenciando Worker
using Empresa.Worker;

// ❌ VIOLAÇÃO: Dependência circular
```

## Fluxo de uma Requisição

```
Cliente (HTTP)
     │
     ▼
┌─────────────────────────────────┐
│     Middleware Pipeline         │
│  (ExceptionHandler, Auth, etc) │
└─────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────┐
│  Endpoint (Minimal API)        │
│  - Recebe DTO de Request       │
│  - Chama Service               │
│  - Retorna IResult             │
└─────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────┐
│  Service (Regras de Negócio)   │
│  - Validações                  │
│  - Orquestração                │
│  - Chama Repository            │
└─────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────┐
│  Repository (Dados)            │
│  - Dapper Query                 │
│  - Retorna Model/Entidade      │
└─────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────┐
│  Oracle Database                │
└─────────────────────────────────┘
```

## Fluxo de um Worker

```
Host.CreateApplicationBuilder
     │
     ▼
┌─────────────────────────────────┐
│  Worker (BackgroundService)    │
│  - ExecuteAsync loop           │
│  - Injeta repositórios/Util    │
└─────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────┐
│  Service/Repository            │
│  - Operações em lote           │
│  - Envio de e-mail (Util)      │
└─────────────────────────────────┘
```

## Padrões de Design Utilizados

### 1. Repository Pattern
```csharp
// Interface na camada Data
public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task<int> CreateAsync(Usuario usuario);
}

// Implementação com Dapper
public class UsuarioRepository : IUsuarioRepository { ... }
```

### 2. Service Layer Pattern
```csharp
// Interface na camada Api
public interface IUsuarioService
{
    Task<UsuarioResponse> GetByIdAsync(int id);
    Task<UsuarioResponse> CreateAsync(UsuarioRequest request);
}
```

### 3. DbSession (Unit of Work simplificado)
```csharp
public class DbSession : IDisposable
{
    public IDbConnection Connection => _connection ??= new OracleConnection(_connectionString);
}
```

### 4. Minimal API Endpoints
```csharp
public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/usuarios");
        grupo.MapGet("/", async (IUsuarioService service) => ...);
    }
}
```

### 5. Injeção de Dependência Nativa
Apenas `Microsoft.Extensions.DependencyInjection`. Nenhum container IoC de terceiros.

## Organização de Namespaces

```
Empresa.Api
├── Endpoints/
├── Services/
├── DTOs/
│   ├── Request/
│   └── Response/
├── Middleware/
└── Program.cs

Empresa.Data
├── Models/
├── Repositories/
├── DbSession.cs
└── ...

Empresa.Util
├── Dados.cs (constantes)
├── Email.cs
├── StringUtils.cs
└── ...

Empresa.Worker
├── Worker.cs
└── Program.cs
```

## Tecnologias e Versões

| Componente | Versão |
|------------|--------|
| .NET SDK | 9.0 |
| ASP.NET Core | 9.0 |
| Dapper | 2.1.79 |
| Oracle.ManagedDataAccess.Core | 23.26.300 |
| Microsoft.Extensions.Hosting | 9.0.9 |
| Microsoft.AspNetCore.OpenApi | 9.0.9 |

## Decisões Arquiteturais (ADRs)

### ADR-001: Minimal API vs Controllers
**Contexto:** Escolha entre Controllers MVC e Minimal APIs.
**Decisão:** Usar Minimal APIs.
**Justificativa:** Menor boilerplate, melhor performance, simplicidade para APIs REST.

### ADR-002: Dapper vs Entity Framework
**Contexto:** Escolha do ORM para acesso a dados.
**Decisão:** Usar Dapper.
**Justificativa:** Performance superior, controle total sobre SQL, ideal para Oracle.

### ADR-003: Injeção de Dependência Nativa
**Contexto:** Escolha do container IoC.
**Decisão:** Usar Microsoft.Extensions.DependencyInjection.
**Justificativa:** Já incluído no .NET, sem dependências externas, suficiente para o escopo.

### ADR-004: Background Worker vs Job Scheduler (Hangfire/Quartz)
**Contexto:** Processamento em background.
**Decisão:** Usar BackgroundService nativo do .NET.
**Justificativa:** Simplicidade, sem dependências externas, controle total do ciclo de vida.

---

> **Documentação mantida pela equipe de arquitetura.**