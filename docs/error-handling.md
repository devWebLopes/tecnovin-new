# ⚠️ Tratamento de Erros — GestãoNew

## Estratégia Geral

O tratamento de erros no GestãoNew segue três níveis:

1. **Middleware Global** — Captura exceções não tratadas
2. **Services** — Regras de negócio com exceções customizadas
3. **Endpoints** — Retorno padronizado com `IResult`

---

## 1. Middleware Global de Exceções

Todas as exceções não tratadas devem ser capturadas por um middleware global, evitando `try-catch` espalhados pelos endpoints.

### Implementação

```csharp
namespace Empresa.Api.Middleware;

/// <summary>
/// Middleware global para captura de exceções não tratadas
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Not Found", ex.Message));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Violação de regra de negócio");
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Business Error", ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Validation Error", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acesso não autorizado");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Forbidden", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                "Internal Server Error", 
                "Ocorreu um erro interno. Tente novamente mais tarde."));
        }
    }
}

/// <summary>
/// Modelo padronizado para respostas de erro
/// </summary>
public record ErrorResponse(string Error, string Message);
```

### Registro no Program.cs

```csharp
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
// ... outros middlewares e endpoints
```

---

## 2. Exceções Customizadas

### Hierarquia de Exceções

```csharp
namespace Empresa.Api.Exceptions;

/// <summary>
/// Base para exceções de negócio do sistema
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Recurso não encontrado (404)
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string resource, object id) 
        : base($"{resource} com ID '{id}' não encontrado(a)") { }
}

/// <summary>
/// Violação de regra de negócio (422)
/// </summary>
public class BusinessException : AppException
{
    public BusinessException(string message) : base(message) { }
}

/// <summary>
/// Erro de validação de dados (400)
/// </summary>
public class ValidationException : AppException
{
    public ValidationException(string message) : base(message) { }
}
```

### Uso nos Services

```csharp
public async Task<UsuarioResponse> GetByIdAsync(int id)
{
    var usuario = await _repository.GetByIdAsync(id);
    
    if (usuario is null)
        throw new NotFoundException("Usuário", id);

    return MapToResponse(usuario);
}

public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
{
    if (await _repository.LoginExistsAsync(request.Login))
        throw new BusinessException("Já existe um usuário cadastrado com este login");

    // ...
}
```

---

## 3. Tratamento em Endpoints

### Padrão com IResult

```csharp
// ✅ Correto: endpoint delega para service e usa IResult
grupo.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
{
    var usuario = await service.GetByIdAsync(id);
    return usuario is null ? Results.NotFound() : Results.Ok(usuario);
});

// ✅ Correto: criações retornam 201
grupo.MapPost("/", async (UsuarioRequest request, IUsuarioService service) =>
{
    var usuario = await service.CreateAsync(request);
    return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
});
```

### Quando usar try-catch no endpoint

- **Raramente** — o middleware global captura exceções não tratadas
- Apenas para cenários específicos onde você precisa de um tratamento diferente

```csharp
grupo.MapPost("/", async (UsuarioRequest request, IUsuarioService service) =>
{
    try
    {
        var usuario = await service.CreateAsync(request);
        return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
    }
    catch (BusinessException ex)
    {
        return Results.UnprocessableEntity(new { error = ex.Message });
    }
});
```

---

## 4. Validação de Entrada

### Data Annotations

```csharp
public class UsuarioRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
}
```

### Validação Manual

```csharp
public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        throw new ValidationException("Nome é obrigatório");

    if (request.Nome.Length < 3)
        throw new ValidationException("Nome deve ter no mínimo 3 caracteres");

    // Regras de negócio...
}
```

---

## 5. Logging de Erros

### Estrutura de Log

| Nível | Quando Usar | Exemplo |
|-------|------------|---------|
| `LogCritical` | Falha catastrófica | Banco de dados indisponível |
| `LogError` | Erro inesperado | Exceção não tratada |
| `LogWarning` | Situação anormal | Recurso não encontrado |
| `LogInformation` | Informação normal | Operação bem-sucedida |
| `LogDebug` | Depuração | Dados de requisição |

### Padrão de Logging

```csharp
// No middleware
_logger.LogError(ex, "Erro ao processar requisição {Method} {Path}", 
    context.Request.Method, context.Request.Path);

// No service
_logger.LogInformation("Usuário {Id} criado com sucesso", usuario.IDUsuario);
_logger.LogWarning("Tentativa de criar usuário com login duplicado: {Login}", request.Login);
```

---

## 6. Respostas de Erro Padronizadas

### Estrutura do ErrorResponse

```json
{
  "error": "Not Found",
  "message": "Usuário com ID '42' não encontrado(a)",
  "traceId": "00-0ab8f9c2d3e4f5a6b7c8d9e0f1a2b3c4-..."
}
```

### Códigos HTTP Utilizados

| Código | Situação |
|--------|----------|
| `200 OK` | Sucesso com body |
| `201 Created` | Recurso criado |
| `204 No Content` | Sucesso sem body |
| `400 Bad Request` | Erro de validação |
| `401 Unauthorized` | Não autenticado |
| `403 Forbidden` | Sem permissão |
| `404 Not Found` | Recurso não encontrado |
| `422 Unprocessable Entity` | Regra de negócio violada |
| `500 Internal Server Error` | Erro interno do servidor |

---

## 7. Boas Práticas

### ✅ Faça
- Use o middleware global para capturar exceções
- Lance exceções customizadas nos services
- Retorne `IResult` padronizado nos endpoints
- Faça logging apropriado em cada nível
- Use mensagens de erro descritivas em português

### ❌ Não Faça
```csharp
// ❌ Catch genérico no endpoint
grupo.MapGet("/{id:int}", async (int id) =>
{
    try {
        var usuario = await service.GetByIdAsync(id);
        return Results.Ok(usuario);
    } catch {
        return Results.Problem("Erro");
    }
});

// ❌ Engolir exceção
catch (Exception ex) { /* não faz nada */ }

// ❌ Usar exceção para controle de fluxo
try {
    var usuario = await service.GetByIdAsync(id);
} catch (NotFoundException) {
    return Results.NotFound();
}
```

---

> **Consulte também:** [coding-standards.md](coding-standards.md) para mais padrões de tratamento de erros no código.