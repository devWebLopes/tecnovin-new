using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints de CRUD para usuários + gerenciamento de vínculos com estabelecimentos
/// </summary>
public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/usuarios")
            .WithTags("Usuários")
            .RequireAuthorization();

        // GET /api/v1/usuarios — Lista todos os usuários
        grupo.MapGet("/", GetAllAsync)
            .WithSummary("Lista todos os usuários")
            .WithDescription("Retorna lista de usuários ordenados por NOME. Parâmetros opcionais: search (filtro por nome), ativo (S/N). Campo senha NUNCA retornado.")
            .Produces<IEnumerable<UsuarioResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/usuarios/{id} — Busca usuário por ID
        grupo.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Busca usuário por ID")
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/v1/usuarios — Cria um novo usuário
        grupo.MapPost("/", CreateAsync)
            .WithSummary("Cria um novo usuário")
            .WithDescription("Senha é hash BCrypt antes de persistir. Login deve ser único.")
            .Produces<UsuarioResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        // PUT /api/v1/usuarios/{id} — Atualiza um usuário existente
        grupo.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Atualiza um usuário existente")
            .WithDescription("Se senha for enviada, aplica novo hash BCrypt. Senha é opcional no update.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        // DELETE /api/v1/usuarios/{id} — Exclusão lógica (ATIVO = 'N')
        grupo.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Exclui logicamente um usuário")
            .WithDescription("Exclusão lógica — seta ATIVO = 'N'. O registro permanece na base.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/v1/usuarios/{id}/estabelecimentos — Árvore de estabelecimentos
        grupo.MapGet("/{id:int}/estabelecimentos", GetEstabelecimentosTreeAsync)
            .WithSummary("Retorna árvore de estabelecimentos com vínculos do usuário")
            .WithDescription("Retorna estrutura hierárquica empresa → estabelecimentos com flag 'vinculado'.")
            .Produces<IEnumerable<EstabelecimentoTreeResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // PUT /api/v1/usuarios/{id}/estabelecimentos — Sincroniza vínculos
        grupo.MapPut("/{id:int}/estabelecimentos", SincronizarEstabelecimentosAsync)
            .WithSummary("Sincroniza vínculos do usuário com estabelecimentos")
            .WithDescription("Recebe dois arrays: vincularEstabelecimentos e desvincularEstabelecimentos. Verifica duplicidade antes de inserir.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> GetAllAsync(IUsuarioService service, string? search = null, string? ativo = null)
    {
        var usuarios = await service.GetAllAsync(search, ativo);
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
        return Results.Created($"/api/v1/usuarios/{usuario.Id}", usuario);
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

    private static async Task<IResult> GetEstabelecimentosTreeAsync(int id, IUsuarioService service)
    {
        var estabs = await service.GetEstabelecimentosTreeAsync(id);
        return Results.Ok(estabs);
    }

    private static async Task<IResult> SincronizarEstabelecimentosAsync(int id, EstabelecimentoVinculoRequest request, IUsuarioService service)
    {
        await service.SincronizarEstabelecimentosAsync(id, request);
        return Results.NoContent();
    }
}