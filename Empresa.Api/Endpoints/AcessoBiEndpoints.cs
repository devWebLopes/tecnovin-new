using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do painel Acesso B.I. — gestão de usuários × empresas (USUARIO_EMPRESA)
/// </summary>
public static class AcessoBiEndpoints
{
    public static void MapAcessoBiEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/acesso-bi")
            .WithTags("Acesso BI");

        // GET /api/v1/acesso-bi/usuarios?search=
        grupo.MapGet("/usuarios", GetUsuariosAsync)
            .RequireAuthorization()
            .WithSummary("Lista usuários com acesso ao B.I.")
            .WithDescription("RF02 — Retorna usuários com ≥1 vínculo em USUARIO_EMPRESA, com agregação LISTAGG das empresas. Filtro opcional por nome via ?search=.")
            .Produces<IEnumerable<UsuarioBiResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/v1/acesso-bi/usuarios/{idUsuario}/empresas
        grupo.MapGet("/usuarios/{idUsuario:long}/empresas", GetEmpresasAsync)
            .RequireAuthorization()
            .WithSummary("Lista empresas vinculadas ao usuário")
            .WithDescription("RF03 — Retorna as empresas vinculadas ao usuário (grid detalhe).")
            .Produces<IEnumerable<EmpresaBiResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/v1/acesso-bi/usuarios/{idUsuario}/empresas/disponiveis
        grupo.MapGet("/usuarios/{idUsuario:long}/empresas/disponiveis", GetEmpresasDisponiveisAsync)
            .RequireAuthorization()
            .WithSummary("Lista empresas NÃO vinculadas ao usuário")
            .WithDescription("RF05 — Retorna empresas ainda não vinculadas ao usuário (dropdown do grid detalhe).")
            .Produces<IEnumerable<EmpresaDisponivelResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        // POST /api/v1/acesso-bi/usuarios
        grupo.MapPost("/usuarios", ConcederAcessoTotalAsync)
            .RequireAuthorization()
            .WithSummary("Concede acesso total ao B.I. (usuário → todas empresas)")
            .WithDescription("RF04 — Insere 1 linha em USUARIO_EMPRESA por empresa de VW_EMPRESA_NEW. Retorna 409 se o usuário já possui acesso.")
            .Produces<IEnumerable<UsuarioBiResponse>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        // POST /api/v1/acesso-bi/usuarios/{idUsuario}/empresas
        grupo.MapPost("/usuarios/{idUsuario:long}/empresas", VincularEmpresaAsync)
            .RequireAuthorization()
            .WithSummary("Vincula uma empresa ao usuário")
            .WithDescription("RF05 — Insere 1 vínculo usuário×empresa. Retorna 409 se o vínculo já existe.")
            .Produces<EmpresaBiResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        // DELETE /api/v1/acesso-bi/empresas/{idUsuarioEmpresa}
        grupo.MapDelete("/empresas/{idUsuarioEmpresa:long}", RemoverAcessoAsync)
            .RequireAuthorization()
            .WithSummary("Remove vínculo de acesso ao B.I.")
            .WithDescription("RF06 — Remove o vínculo pela PK real ID_USUARIO_EMPRESA (correção do defeito D2 do legado).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static int? GetIdPerfil(HttpContext context)
    {
        var claimIdPerfil = context.User.FindFirst("ID_PERFIL")?.Value;
        if (string.IsNullOrEmpty(claimIdPerfil) || !int.TryParse(claimIdPerfil, out var idPerfil))
            return null;
        return idPerfil;
    }

    private static async Task<IResult> GetUsuariosAsync(string? search, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        var usuarios = await service.GetUsuariosComAcessoAsync(idPerfil.Value, search);
        return Results.Ok(usuarios);
    }

    private static async Task<IResult> GetEmpresasAsync(long idUsuario, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        var empresas = await service.GetEmpresasDoUsuarioAsync(idPerfil.Value, idUsuario);
        return Results.Ok(empresas);
    }

    private static async Task<IResult> GetEmpresasDisponiveisAsync(long idUsuario, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        var empresas = await service.GetEmpresasDisponiveisAsync(idPerfil.Value, idUsuario);
        return Results.Ok(empresas);
    }

    private static async Task<IResult> ConcederAcessoTotalAsync(
        ConcederAcessoTotalRequest request, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        var usuarios = await service.ConcederAcessoTotalAsync(idPerfil.Value, request);
        return Results.Created("/api/v1/acesso-bi/usuarios", usuarios);
    }

    private static async Task<IResult> VincularEmpresaAsync(
        long idUsuario, VincularEmpresaRequest request, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        var vinculo = await service.VincularEmpresaAsync(idPerfil.Value, idUsuario, request);
        return Results.Created($"/api/v1/acesso-bi/usuarios/{idUsuario}/empresas", vinculo);
    }

    private static async Task<IResult> RemoverAcessoAsync(
        long idUsuarioEmpresa, IAcessoBiService service, HttpContext context)
    {
        var idPerfil = GetIdPerfil(context);
        if (idPerfil is null)
            return Results.Unauthorized();

        await service.RemoverAcessoAsync(idPerfil.Value, idUsuarioEmpresa);
        return Results.NoContent();
    }
}