using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints de CRUD para páginas/menu
/// </summary>
public static class PaginaEndpoints
{
    public static void MapPaginaEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/paginas")
            .WithTags("Páginas");

        // GET /api/v1/paginas/menu-hierarquico — Menu hierárquico do perfil
        grupo.MapGet("/menu-hierarquico", GetMenuHierarquicoAsync)
            .RequireAuthorization()
            .WithSummary("Retorna o menu hierárquico do perfil do usuário")
            .WithDescription("Retorna a árvore de páginas que o perfil do usuário tem acesso, extraindo ID_PERFIL do token JWT.")
            .Produces<IEnumerable<PaginaTreeResponse>>(StatusCodes.Status200OK);

        grupo.MapGet("/", GetAllAsync)
            .RequireAuthorization()
            .WithSummary("Lista todas as páginas")
            .Produces<IEnumerable<PaginaResponse>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:int}", GetByIdAsync)
            .RequireAuthorization()
            .WithSummary("Busca página por ID")
            .Produces<PaginaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/v1/paginas/{chaveControle}/autorizacao — guard de rota do frontend (RF04)
        // Protegido pela policy "PaginaAcesso" (RN-12): 200 se o perfil tem permissão, 403 caso contrário.
        grupo.MapGet("/{chaveControle}/autorizacao", VerificarAutorizacaoAsync)
            .RequireAuthorization("PaginaAcesso")
            .WithSummary("Verifica se o perfil possui acesso à página")
            .WithDescription("RF04 — Retorna 200 se o perfil do JWT possui vínculo em ACESSO_PERFIL_PAGINA " +
                             "para a CHAVE_CONTROLE informada; 403 caso contrário. Usado pelo guard de " +
                             "rota do frontend (defesa em profundidade — o backend é a autoridade).")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPost("/", CreateAsync)
            .WithSummary("Cria uma nova página")
            .Produces<PaginaResponse>(StatusCodes.Status201Created);

        grupo.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Atualiza uma página existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Exclui logicamente uma página")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetMenuHierarquicoAsync(IPaginaService service, HttpContext context)
    {
        // Extrair ID_PERFIL do JWT claim
        var claimIdPerfil = context.User.FindFirst("ID_PERFIL")?.Value;
        if (string.IsNullOrEmpty(claimIdPerfil) || !int.TryParse(claimIdPerfil, out var idPerfil))
            return Results.Unauthorized();

        var paginas = await service.GetMenuHierarquicoAsync(idPerfil);
        return Results.Ok(paginas);
    }

    private static async Task<IResult> GetAllAsync(IPaginaService service)
    {
        var paginas = await service.GetAllAsync();
        return Results.Ok(paginas);
    }

    private static async Task<IResult> GetByIdAsync(int id, IPaginaService service)
    {
        var pagina = await service.GetByIdAsync(id);
        return pagina is null ? Results.NotFound() : Results.Ok(pagina);
    }

    private static Task<IResult> VerificarAutorizacaoAsync(string chaveControle, HttpContext context)
    {
        // Chega aqui apenas se a policy "PaginaAcesso" teve sucesso (senão 403).
        return Task.FromResult<IResult>(Results.Ok(new
        {
            autorizado = true,
            chaveControle
        }));
    }

    private static async Task<IResult> CreateAsync(PaginaRequest request, IPaginaService service)
    {
        var pagina = await service.CreateAsync(request);
        return Results.Created($"/api/v1/paginas/{pagina.Id}", pagina);
    }

    private static async Task<IResult> UpdateAsync(int id, PaginaRequest request, IPaginaService service)
    {
        var result = await service.UpdateAsync(id, request);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeleteAsync(int id, IPaginaService service)
    {
        var result = await service.DeleteAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
}