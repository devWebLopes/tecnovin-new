using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints de CRUD para perfis + gerenciamento de permissões de páginas
/// </summary>
public static class PerfilEndpoints
{
    public static void MapPerfilEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/perfis")
            .WithTags("Perfis")
            .RequireAuthorization();

        // GET /api/v1/perfis — Lista todos os perfis (com search opcional)
        grupo.MapGet("/", GetAllAsync)
            .WithSummary("Lista todos os perfis")
            .WithDescription("Retorna lista de perfis. Parâmetro 'search' opcional para filtrar por descrição.")
            .Produces<IEnumerable<PerfilResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/perfis/lista-simples — Lista simplificada para dropdown
        grupo.MapGet("/lista-simples", GetListaSimplesAsync)
            .WithSummary("Lista simplificada de perfis para dropdown")
            .WithDescription("Retorna apenas ID e descrição dos perfis, usado para popular combo boxes no frontend.")
            .Produces<IEnumerable<PerfilResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/perfis/{id} — Busca perfil por ID
        grupo.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Busca perfil por ID")
            .Produces<PerfilResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/v1/perfis — Cria um novo perfil
        grupo.MapPost("/", CreateAsync)
            .WithSummary("Cria um novo perfil")
            .Produces<PerfilResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        // PUT /api/v1/perfis/{id} — Atualiza um perfil existente
        grupo.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Atualiza um perfil existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        // DELETE /api/v1/perfis/{id} — Exclui fisicamente um perfil
        grupo.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Exclui fisicamente um perfil")
            .WithDescription("DELETE físico da tabela ACESSO_CADASTRO_PERFIL. Retorna 409 se houver conflito.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/v1/perfis/{id}/paginas — Árvore hierárquica de páginas com vínculos
        grupo.MapGet("/{id:int}/paginas", GetPaginasTreeAsync)
            .WithSummary("Retorna árvore de páginas com permissões do perfil")
            .WithDescription("Retorna estrutura hierárquica de páginas com flag 'vinculado' indicando permissões do perfil.")
            .Produces<IEnumerable<PaginaTreeResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // PUT /api/v1/perfis/{id}/paginas — Sincroniza permissões do perfil
        grupo.MapPut("/{id:int}/paginas", SalvarPermissoesAsync)
            .WithSummary("Sincroniza permissões de páginas do perfil")
            .WithDescription("Operação idempotente usando MERGE Oracle. Recebe dois arrays: vincularIds e desvincularIds.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> GetAllAsync(IPerfilService service, string? search = null)
    {
        var perfis = await service.GetAllAsync(search);
        return Results.Ok(perfis);
    }

    private static async Task<IResult> GetListaSimplesAsync(IPerfilService service)
    {
        var perfis = await service.GetAllAsync();
        return Results.Ok(perfis);
    }

    private static async Task<IResult> GetByIdAsync(int id, IPerfilService service)
    {
        var perfil = await service.GetByIdAsync(id);
        return perfil is null ? Results.NotFound() : Results.Ok(perfil);
    }

    private static async Task<IResult> CreateAsync(PerfilRequest request, IPerfilService service)
    {
        var perfil = await service.CreateAsync(request);
        return Results.Created($"/api/v1/perfis/{perfil.IdPerfil}", perfil);
    }

    private static async Task<IResult> UpdateAsync(int id, PerfilRequest request, IPerfilService service)
    {
        var result = await service.UpdateAsync(id, request);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeleteAsync(int id, IPerfilService service)
    {
        var result = await service.DeleteAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetPaginasTreeAsync(int id, IPerfilService service)
    {
        var paginas = await service.GetPaginasTreeAsync(id);
        return Results.Ok(paginas);
    }

    private static async Task<IResult> SalvarPermissoesAsync(int id, PerfilPaginasRequest request, IPerfilService service)
    {
        await service.SalvarPermissoesAsync(id, request);
        return Results.NoContent();
    }
}