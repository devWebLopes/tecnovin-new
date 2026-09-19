# 🔌 Skill: Minimal API Patterns — .NET 9

## Sobre
Esta skill define o padrão estrito para criação de endpoints usando Minimal APIs no projeto GestaoNew. Todo endpoint deve seguir estas convenções.

## Estrutura de Arquivo

Cada módulo de domínio deve ter um arquivo estático em `Empresa.Api/Endpoints/`:

```
Empresa.Api/Endpoints/
├── AuthEndpoints.cs
├── UsuarioEndpoints.cs
├── PerfilEndpoints.cs
├── PaginaEndpoints.cs
├── EstabelecimentoEndpoints.cs
├── ComprasEndpoints.cs
├── VendasEndpoints.cs
├── FinanceiroEndpoints.cs
├── PrazoMedioEndpoints.cs
└── SecundariosEndpoints.cs
```

## Template de Endpoint

```csharp
using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

public static class ModuloEndpoints
{
    public static void MapModuloEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/modulo")
            .RequireAuthorization()  // Sempre proteger com JWT
            .WithTags("Módulo")       // Tag para agrupamento no Swagger
            .WithOpenApi();

        // GET - Listar todos
        grupo.MapGet("/", async (IModuloService service) =>
        {
            var resultado = await service.GetAllAsync();
            return Results.Ok(resultado);
        })
        .WithSummary("Lista todos os registros do módulo")
        .WithDescription("Retorna uma lista paginada de registros ativos.")
        .Produces<IEnumerable<ModuloResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET - Por ID
        grupo.MapGet("/{id:int}", async (int id, IModuloService service) =>
        {
            var resultado = await service.GetByIdAsync(id);
            return resultado is null
                ? Results.NotFound(new { mensagem = "Registro não encontrado" })
                : Results.Ok(resultado);
        })
        .WithSummary("Obtém um registro pelo ID")
        .Produces<ModuloResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST - Criar
        grupo.MapPost("/", async (ModuloRequest request, IModuloService service) =>
        {
            var resultado = await service.CreateAsync(request);
            return Results.Created($"/api/v1/modulo/{resultado.Id}", resultado);
        })
        .WithSummary("Cria um novo registro")
        .WithDescription("Cria um registro com validação de regras de negócio.")
        .Produces<ModuloResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // PUT - Atualizar
        grupo.MapPut("/{id:int}", async (int id, ModuloRequest request, IModuloService service) =>
        {
            var resultado = await service.UpdateAsync(id, request);
            return resultado is null
                ? Results.NotFound(new { mensagem = "Registro não encontrado" })
                : Results.Ok(resultado);
        })
        .WithSummary("Atualiza um registro existente")
        .Produces<ModuloResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE - Exclusão lógica
        grupo.MapDelete("/{id:int}", async (int id, IModuloService service) =>
        {
            var sucesso = await service.DeleteAsync(id);
            return sucesso
                ? Results.NoContent()
                : Results.NotFound(new { mensagem = "Registro não encontrado" });
        })
        .WithSummary("Remove logicamente um registro (ATIVO='N')")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
```

## Regras Obrigatórias

### 1. Versionamento de API
- **Sempre** usar prefixo `/api/v1/` para todos os endpoints
- Preparar estrutura para `/api/v2/` quando houver breaking changes

### 2. Retorno Padronizado
- **Sempre** retornar `IResult` (nunca o objeto diretamente)
- Usar helpers: `Results.Ok()`, `Results.Created()`, `Results.NoContent()`, `Results.NotFound()`, `Results.BadRequest()`

### 3. Autenticação
- **Sempre** adicionar `.RequireAuthorization()` no MapGroup
- Exceto: `/api/v1/auth/login` e `/api/v1/health`

### 4. Documentação Swagger
- **Sempre** usar `.WithSummary()` e `.WithDescription()`
- **Sempre** usar `.Produces<T>()` para documentar códigos de resposta
- **Sempre** usar `.WithTags()` para agrupamento lógico

### 5. Injeção de Dependência
- Services são injetados diretamente nos delegates dos endpoints
- NUNCA instanciar services ou repositories manualmente

### 6. Validação
- Validações de negócio DEVEM estar nos Services, NUNCA nos endpoints
- Endpoints apenas delegam para o service e mapeiam o resultado HTTP

### 7. Registro no Program.cs
```csharp
// Em Program.cs, após builder.Build():
var app = builder.Build();

app.MapAuthEndpoints();
app.MapUsuarioEndpoints();
app.MapPerfilEndpoints();
app.MapPaginaEndpoints();
// ... etc
```

## Anti-Padrões (Proibidos)

```csharp
// ❌ NUNCA expor entidade do banco diretamente
grupo.MapGet("/{id}", async (int id, IUsuarioRepository repo) =>
{
    var usuario = await repo.GetByIdAsync(id);
    return Results.Ok(usuario); // Usuario é model do banco!
});

// ❌ NUNCA fazer regra de negócio no endpoint
grupo.MapPost("/", async (UsuarioRequest req, IUsuarioService svc) =>
{
    if (req.Senha.Length < 6) // ← Isso é regra de negócio, deve estar no Service
        return Results.BadRequest();
    // ...
});

// ❌ NUNCA esquecer .RequireAuthorization()
var grupo = app.MapGroup("/api/v1/usuarios"); // Sem proteção JWT!

// ✅ Sempre proteger, exceto auth e health
var grupo = app.MapGroup("/api/v1/usuarios").RequireAuthorization();
```

## Padrão para Endpoints de Relatório (Download)

```csharp
grupo.MapGet("/exportar", async (IModuloService service) =>
{
    var arquivo = await service.ExportarAsync();
    return Results.File(arquivo, "application/pdf", "relatorio.pdf");
})
.WithSummary("Exporta relatório em PDF")
.Produces(StatusCodes.Status200OK, contentType: "application/pdf");
```

## Padrão para Endpoints com Query Parameters Complexos

```csharp
grupo.MapGet("/busca", async (
    [FromQuery] string? termo,
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanho = 20,
    [FromQuery] string? ordenarPor,
    IModuloService service) =>
{
    var resultado = await service.BuscarAsync(termo, pagina, tamanho, ordenarPor);
    return Results.Ok(resultado);
})
.WithSummary("Busca paginada com filtros");