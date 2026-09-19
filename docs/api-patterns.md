# 🌐 Padrões de API — GestãoNew

## Visão Geral

O projeto **GestãoNew** utiliza **Minimal APIs** (introduzidas no .NET 6+) em vez de Controllers tradicionais. Este documento define os padrões para criação de endpoints.

---

## Estrutura de Endpoints

### Organização por Domínio

Cada domínio de negócio (Usuário, Página, Perfil, etc.) deve ter seu próprio arquivo de endpoints:

```
Empresa.Api/Endpoints/
├── UsuarioEndpoints.cs
├── PaginaEndpoints.cs
├── PerfilEndpoints.cs
└── ...
```

### Template de Endpoint

```csharp
namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints para gerenciamento de usuários
/// </summary>
public static class UsuarioEndpoints
{
    /// <summary>
    /// Registra todos os endpoints de usuário no pipeline
    /// </summary>
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuários");

        // GET /api/usuarios
        grupo.MapGet("/", GetAllAsync);

        // GET /api/usuarios/{id}
        grupo.MapGet("/{id:int}", GetByIdAsync);

        // POST /api/usuarios
        grupo.MapPost("/", CreateAsync);

        // PUT /api/usuarios/{id}
        grupo.MapPut("/{id:int}", UpdateAsync);

        // DELETE /api/usuarios/{id}
        grupo.MapDelete("/{id:int}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(IUsuarioService service)
    {
        var usuarios = await service.GetAllAsync();
        return Results.Ok(usuarios);
    }

    private static async Task<IResult> GetByIdAsync(int id, IUsuarioService service)
    {
        var usuario = await service.GetByIdAsync(id);
        return usuario is null ? Results.NotFound() : Results.Ok(usuario);
    }

    private static async Task<IResult> CreateAsync(UsuarioRequest request, IUsuarioService service)
    {
        var usuario = await service.CreateAsync(request);
        return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
    }

    private static async Task<IResult> UpdateAsync(int id, UsuarioRequest request, IUsuarioService service)
    {
        var result = await service.UpdateAsync(id, request);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeleteAsync(int id, IUsuarioService service)
    {
        var result = await service.DeleteAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
}
```

---

## Registro dos Endpoints

No `Program.cs`, registre os endpoints via extension methods:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Registro dos endpoints
app.MapUsuarioEndpoints();
app.MapPaginaEndpoints();

app.Run();
```

---

## Padrão de Endpoints CRUD

| Método | Rota | Função | Retorno |
|--------|------|--------|---------|
| `GET` | `/api/usuarios` | Listar todos | `200 OK` + lista |
| `GET` | `/api/usuarios/{id}` | Buscar por ID | `200 OK` ou `404 Not Found` |
| `POST` | `/api/usuarios` | Criar | `201 Created` + location header |
| `PUT` | `/api/usuarios/{id}` | Atualizar | `204 No Content` ou `404 Not Found` |
| `DELETE` | `/api/usuarios/{id}` | Deletar | `204 No Content` ou `404 Not Found` |

---

## Padrões de Retorno (IResult)

### Sucesso
```csharp
// 200 OK com body
return Results.Ok(usuario);

// 201 Created com location
return Results.Created($"/api/usuarios/{id}", usuario);

// 204 No Content (sem body)
return Results.NoContent();
```

### Erro
```csharp
// 404 Not Found
return Results.NotFound();

// 400 Bad Request
return Results.BadRequest("Mensagem de erro");

// 422 Unprocessable Entity
return Results.UnprocessableEntity(erros);

// 500 Internal Server Error (via middleware)
throw new Exception("Erro inesperado"); // Capturado pelo middleware global
```

---

## DTOs — Data Transfer Objects

### Request DTO (entrada)
```csharp
/// <summary>
/// DTO para criação/atualização de usuário
/// </summary>
public class UsuarioRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Senha { get; set; } = string.Empty;

    public int IDPerfil { get; set; }
}
```

### Response DTO (saída)
```csharp
/// <summary>
/// DTO de resposta com dados do usuário
/// </summary>
public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
```

### Regras para DTOs
- **Request DTOs:** Anotados com `System.ComponentModel.DataAnnotations`
- **Response DTOs:** Sem referências a entidades do banco
- **Nunca** expor entidades `Empresa.Data.Models` diretamente nos endpoints
- Clone de propriedades é aceitável (não precisa de AutoMapper se simples)

---

## Validação de Entrada

### Validação com Data Annotations (embutida no ASP.NET)
```csharp
grupo.MapPost("/", async (UsuarioRequest request, IUsuarioService service) =>
{
    if (!ValidationHelper.TryValidate(request, out var errors))
        return Results.UnprocessableEntity(errors);

    var usuario = await service.CreateAsync(request);
    return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
});
```

### Validação Customizada no Service
```csharp
public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
{
    // Validação de negócio
    if (await _repository.LoginExistsAsync(request.Login))
        throw new BusinessException("Login já cadastrado");

    // Mapeamento
    var usuario = new Usuario
    {
        Nome = request.Nome,
        Login = request.Login,
        Senha = HashPassword(request.Senha),
        Ativo = "S"
    };

    var id = await _repository.CreateAsync(usuario);
    return MapToResponse(usuario);
}
```

---

## Endpoints com Paginação

```csharp
// Request
public class PaginatedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}

// Response
public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// Endpoint
grupo.MapGet("/", async ([AsParameters] PaginatedRequest request, IUsuarioService service) =>
{
    var result = await service.GetPaginatedAsync(request);
    return Results.Ok(result);
});
```

---

## Versionamento de API

Para versionamento, use prefixo na rota:

```csharp
// Versão 1
var grupo = app.MapGroup("/api/v1/usuarios");

// Versão 2 (quando houver breaking changes)
var grupo = app.MapGroup("/api/v2/usuarios");
```

---

## Boas Práticas

### 1. Nomeação de Endpoints
```csharp
// ✅ Correto
grupo.MapGet("/", ...);                  // Listar
grupo.MapGet("/{id:int}", ...);          // Buscar por ID
grupo.MapPost("/", ...);                 // Criar
grupo.MapPut("/{id:int}", ...);          // Atualizar
grupo.MapDelete("/{id:int}", ...);       // Deletar

// Para ações específicas
grupo.MapPost("/{id:int}/ativar", ...);  // Ação
grupo.MapPost("/{id:int}/resetar-senha", ...); // Ação com kebab-case
```

### 2. Filtros e Busca
```csharp
// Filtros como query parameters
grupo.MapGet("/", async (string? nome, bool? ativo, IUsuarioService service) =>
{
    var usuarios = await service.GetFilteredAsync(nome, ativo);
    return Results.Ok(usuarios);
});

// Rota: GET /api/usuarios?nome=João&ativo=true
```

### 3. Organização de Métodos
- Métodos privados estáticos no mesmo arquivo
- Cada método de handler = 1 responsabilidade
- Handlers curtos (máx 15-20 linhas)

### 4. Documentação com OpenAPI
```csharp
grupo.MapGet("/{id:int}", GetByIdAsync)
    .WithName("GetUsuarioById")
    .WithDescription("Retorna um usuário específico pelo ID")
    .Produces<UsuarioResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
```

---

## ⚠️ Anti-Padrões

### ❌ Proibido
```csharp
// ❌ Usar entidade do banco diretamente
grupo.MapGet("/", async (IUsuarioRepository repo) =>
{
    return Results.Ok(await repo.GetAllAsync()); // Expõe Senha!
});

// ❌ Lógica de negócio no endpoint
grupo.MapPost("/", async (UsuarioRequest request) =>
{
    if (string.IsNullOrEmpty(request.Nome))
        return Results.BadRequest();
    // ... regras de negócio no endpoint
});

// ❌ Métodos síncronos
grupo.MapGet("/", (IUsuarioService service) =>
{
    var usuarios = service.GetAllAsync().Result; // ❌ Blocking call
});
```

---

## Exemplo Completo

Veja o exemplo implementado em `Empresa.Api/Program.cs` com o endpoint `GET /weatherforecast` (template temporário).

---

> **Consulte também:** [data-layer.md](data-layer.md) para padrões de repositório e acesso a dados.