# 🛡️ Skill: Security Patterns — .NET 9

## Sobre
Esta skill define os padrões obrigatórios de segurança para o projeto GestaoNew. Cobre proteção contra SQL Injection, CORS, HTTPS enforcement, headers de segurança, rate limiting e validação de entrada.

## 1. Proteção contra SQL Injection

### Regra de Ouro: Nunca Concatenar SQL

```csharp
// ❌ MORTAL — SQL Injection garantido
var sql = $"SELECT * FROM USUARIO WHERE LOGIN = '{login}' AND SENHA = '{senha}'";

// ❌ GRAVE — Ainda vulnerável a injection
var sql = $"SELECT * FROM USUARIO WHERE LOGIN = '{login}'";

// ✅ CORRETO — Dapper com bind variables
var sql = "SELECT * FROM USUARIO WHERE LOGIN = :Login AND SENHA = :Senha";
return await connection.QueryFirstOrDefaultAsync<Usuario>(sql,
    new { Login = login, Senha = senha });

// ✅ CORRETO — DynamicParameters
var parametros = new DynamicParameters();
parametros.Add("Login", login, DbType.String);
parametros.Add("Senha", senha, DbType.String);
return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, parametros);
```

> ⚠️ **Zero tolerância**: Nenhuma query SQL pode conter concatenação de strings com input do usuário. Code review deve rejeitar automaticamente qualquer `$"SELECT...{var}"`.

## 2. CORS — Cross-Origin Resource Sharing

```csharp
// Em Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Restrita", policy =>
    {
        policy
            .WithOrigins(
                // Apenas origens conhecidas
                "https://gestaonew.tecnovin.com.br",
                "http://localhost:5173"  // Dev frontend
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Necessário para JWT via cookie (opcional)
    });
});

// Aplicar antes de UseAuthentication/UseAuthorization
app.UseCors("Restrita");
```

> ⚠️ **NUNCA** usar `.AllowAnyOrigin()` em produção. Listar explicitamente as origens permitidas.

## 3. HTTPS Enforcement

```csharp
// Em Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}

// Configurar HSTS com opções seguras
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

## 4. Headers de Segurança

```csharp
// Em Program.cs — Middleware de headers de segurança
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    // Previne MIME-type sniffing
    headers["X-Content-Type-Options"] = "nosniff";

    // Previne clickjacking
    headers["X-Frame-Options"] = "DENY";

    // Habilita XSS filter no navegador
    headers["X-XSS-Protection"] = "1; mode=block";

    // Referrer Policy
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Permissions Policy (restringe APIs do navegador)
    headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), interest-cohort=()";

    // Content Security Policy
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "connect-src 'self'";

    await next();
});
```

## 5. Rate Limiting

```csharp
// Em Program.cs
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    // Política para endpoints de autenticação
    options.AddFixedWindowLimiter("Auth", config =>
    {
        config.PermitLimit = 5;           // 5 requisições
        config.Window = TimeSpan.FromMinutes(1); // por minuto
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;            // Sem fila (rejeita imediatamente)
    });

    // Política global para todos os endpoints
    options.AddFixedWindowLimiter("Global", config =>
    {
        config.PermitLimit = 100;          // 100 requisições
        config.Window = TimeSpan.FromMinutes(1); // por minuto por IP
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 20;            // Fila de até 20
    });

    // Política para relatórios pesados (financeiro, DRE)
    options.AddFixedWindowLimiter("Relatorios", config =>
    {
        config.PermitLimit = 10;           // 10 requisições
        config.Window = TimeSpan.FromMinutes(1); // por minuto
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Aplicar rate limiter
app.UseRateLimiter();

// Uso nos endpoints:
grupo.MapPost("/login", ...)
    .RequireRateLimiting("Auth");  // 5 tentativas/min

grupo.MapGet("/financeiro/dre", ...)
    .RequireRateLimiting("Relatorios");  // Protege queries pesadas
```

## 6. Validação de Entrada

```csharp
// Validação no Service (camada de negócio)
public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
{
    // Validação de campos obrigatórios
    if (string.IsNullOrWhiteSpace(request.Nome))
        throw new ArgumentException("Nome é obrigatório");

    if (string.IsNullOrWhiteSpace(request.Login))
        throw new ArgumentException("Login é obrigatório");

    // Validação de tamanho
    if (request.Nome.Length > 100)
        throw new ArgumentException("Nome não pode exceder 100 caracteres");

    if (request.Login.Length > 50)
        throw new ArgumentException("Login não pode exceder 50 caracteres");

    // Validação de senha (complexidade)
    if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
        throw new ArgumentException("Senha deve ter no mínimo 6 caracteres");

    // Sanitização contra XSS (remoção de HTML tags)
    request.Nome = Sanitizar(request.Nome);
    request.Login = Sanitizar(request.Login);

    // ... continua criação ...
}

// Sanitização básica contra XSS
private static string Sanitizar(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    return System.Net.WebUtility.HtmlEncode(input)
        .Replace("'", "''"); // Escape Oracle single quote como defesa em profundidade
}
```

## 7. Proteção de Dados Sensíveis

```csharp
// ⚠️ NUNCA logar dados sensíveis
public class UsuarioService
{
    // ❌ Catastrófico — senha no log
    _logger.LogInformation("Criando usuário: Login={Login}, Senha={Senha}", login, senha);

    // ✅ Correto — apenas dados não-sensíveis
    _logger.LogInformation("Criando usuário: Login={Login}", login);

    // ⚠️ NUNCA retornar senha ou hash em DTOs
    public record UsuarioResponse(
        int Id,
        string Nome,
        string Login,
        string Ativo
        // ❌ Sem campo Senha ou SenhaHash!
    );
}

// ⚠️ NUNCA expor connection string em logs ou respostas
catch (OracleException ex)
{
    _logger.LogError(ex, "Erro Oracle: {Message}", ex.Message);
    // ❌ NUNCA: _logger.LogError("ConnString: {ConnStr}", connectionString);
}
```

## 8. Proteção contra Enumeração de Usuários

```csharp
// ❌ Errado — revela se usuário existe ou não
if (usuario is null)
    return Results.NotFound("Usuário não encontrado"); // Vaza informação

// ✅ Correto — resposta genérica
if (usuario is null || !BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
{
    _logger.LogWarning("Tentativa de login falhou para: {Login}", request.Login);
    return Results.Unauthorized(); // Mesma resposta para usuário inexistente ou senha errada
}
```

## 9. Proteção CSRF (para cookies JWT)

Se optar por armazenar JWT em cookie HttpOnly (mais seguro que localStorage):

```csharp
// Configuração do cookie JWT
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;       // Inacessível via JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Apenas HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Previne CSRF
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Proteção anti-forgery para formulários
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Precisa ser lido pelo JS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```

## 10. Sanitização de Output (XSS Prevention)

```csharp
// No frontend React (NUNCA usar dangerouslySetInnerHTML sem sanitização)
// Sempre usar escape automático do React (JSX faz escape por padrão)

// ❌ Perigoso — XSS se dados contiverem <script>
<div dangerouslySetInnerHTML={{ __html: usuario.nome }} />

// ✅ Seguro — React faz escape automático
<div>{usuario.nome}</div>

// Se for absolutamente necessário renderizar HTML, usar DOMPurify:
// import DOMPurify from 'dompurify';
// <div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(html) }} />
```

## Checklist de Segurança (Code Review)

Todo pull request deve passar por estas verificações:

### SQL Injection
- [ ] Nenhuma concatenação de string em queries SQL
- [ ] Todas as queries usam `DynamicParameters` ou anonymous objects
- [ ] Nomes de tabela/coluna dinâmicos validados contra whitelist

### Autenticação
- [ ] Senhas hasheadas com BCrypt (WorkFactor ≥ 12)
- [ ] Nenhum campo de senha em DTOs de Response
- [ ] Login com resposta genérica (sem distinguir "usuário não existe" vs "senha incorreta")
- [ ] JWT Secret fora do código (User Secrets / variável de ambiente)
- [ ] Rate limiting em `/auth/login`

### Dados Sensíveis
- [ ] Nenhum dado sensível em logs (senhas, tokens, connection strings)
- [ ] DTOs não expõem campos sensíveis do modelo
- [ ] `appsettings.Development.json` no `.gitignore` (se contiver secrets)

### Headers e Transporte
- [ ] CORS com origens explícitas (sem `AllowAnyOrigin` em produção)
- [ ] HTTPS redirection em produção
- [ ] HSTS configurado
- [ ] Headers de segurança: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection

### Validação
- [ ] Todos os inputs validados no Service
- [ ] Tamanho máximo verificado para campos string
- [ ] Caracteres especiais sanitizados
- [ ] Tipos validados (int, decimal, DateTime)

## Resposta a Incidentes

Em caso de suspeita de violação de segurança:

1. **Revogar** imediatamente todos os tokens JWT ativos (alterar JWT Secret)
2. **Verificar** logs de acesso (`/logs/` e tabela `ACESSO_VISUALIZACAO_PAGINA`)
3. **Resetar** senha de usuários afetados
4. **Auditar** queries executadas no Oracle (`V$SQL`)
5. **Reportar** incidente conforme política da TreisTecnovin