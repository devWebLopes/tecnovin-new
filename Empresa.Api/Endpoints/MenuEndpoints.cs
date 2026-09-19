using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do módulo Menu — shell de navegação do usuário autenticado.
/// </summary>
public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/menu")
            .WithTags("Menu");

        // GET /api/v1/menu
        grupo.MapGet("/", GetMenuAsync)
            .RequireAuthorization()
            .WithSummary("Retorna o shell do menu (árvore + mais acessados + usuário/B.I.)")
            .WithDescription("RF01/RF02/RF05 — Árvore hierárquica do perfil (cacheada), top 10 " +
                             "mais acessados e barra do usuário com link condicional do B.I. " +
                             "Extrai ID_USUARIO e ID_PERFIL do token JWT (stateless).")
            .Produces<MenuResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        // POST /api/v1/menu/acessos
        grupo.MapPost("/acessos", RegistrarAcessoAsync)
            .RequireAuthorization()
            .WithSummary("Registra telemetria de acesso a uma página")
            .WithDescription("RF03 — Body { chaveControle }. Valida a permissão da chave (RN-12) " +
                             "antes do upsert MERGE em ACESSO_VISUALIZACAO_PAGINA (RN-08). " +
                             "Acesso não autorizado retorna 403 e NÃO é registrado.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static int? GetIdPerfil(HttpContext context)
    {
        var claim = context.User.FindFirst("ID_PERFIL")?.Value;
        return string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id) ? null : id;
    }

    private static int? GetIdUsuario(HttpContext context)
    {
        var claim = context.User.FindFirst("ID_USUARIO")?.Value;
        return string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id) ? null : id;
    }

    private static async Task<IResult> GetMenuAsync(IMenuService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        var idUsuario = GetIdUsuario(context);
        if (idPerfil is null || idUsuario is null)
            return Results.Unauthorized();

        // RF01.5/D-09: erro de banco propaga como 500 (middleware) — nunca vira redirect de login.
        var menu = await service.GetMenuAsync(idUsuario.Value, idPerfil.Value);
        return Results.Ok(menu);
    }

    private static async Task<IResult> RegistrarAcessoAsync(
        RegistroAcessoMenuRequest request, IMenuService service, HttpContext context)
    {
        // RF08.2: chaveControle obrigatória
        if (string.IsNullOrWhiteSpace(request.ChaveControle))
            return Results.BadRequest(new { message = "chaveControle é obrigatória." });

        var idPerfil = GetIdPerfil(context);
        var idUsuario = GetIdUsuario(context);
        if (idPerfil is null || idUsuario is null)
            return Results.Unauthorized();

        // Sem permissão → serviço ignora silenciosamente (D-15); retorna 204 independentemente.
        await service.RegistrarAcessoAsync(request.ChaveControle, idUsuario.Value, idPerfil.Value);
        return Results.NoContent();
    }
}
