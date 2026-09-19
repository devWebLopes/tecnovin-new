using Empresa.Data.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Empresa.Api.Authorization;

/// <summary>
/// Handler da policy "PaginaAcesso" — tradução do guard GetPaginaByChave das ~60 páginas
/// internas do legado (RN-12): lê o ID_PERFIL do JWT, extrai a CHAVE_CONTROLE da rota e
/// consulta o vínculo perfil×página no banco. O backend é a autoridade (RF04.5).
/// </summary>
public class PaginaAutorizacaoHandler : AuthorizationHandler<PaginaAutorizacaoRequirement>
{
    private readonly IAcessoRepository _acessoRepository;
    private readonly ILogger<PaginaAutorizacaoHandler> _logger;

    public PaginaAutorizacaoHandler(
        IAcessoRepository acessoRepository,
        ILogger<PaginaAutorizacaoHandler> logger)
    {
        _acessoRepository = acessoRepository;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PaginaAutorizacaoRequirement requirement)
    {
        var claimPerfil = context.User.FindFirst("ID_PERFIL")?.Value;
        if (string.IsNullOrEmpty(claimPerfil) || !int.TryParse(claimPerfil, out var idPerfil))
        {
            context.Fail();
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            context.Fail();
            return;
        }

        // CHAVE_CONTROLE vem do route value {chaveControle} ou da query string ?chaveControle=
        var chave = httpContext.Request.RouteValues["chaveControle"]?.ToString()
                    ?? httpContext.Request.Query["chaveControle"].ToString();

        if (string.IsNullOrEmpty(chave))
        {
            context.Fail();
            return;
        }

        var pagina = await _acessoRepository.GetPaginaByChaveAsync(chave, idPerfil);
        if (pagina is not null)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Autorização negada — chave '{Chave}' sem permissão para o perfil {Perfil}",
                chave, idPerfil);
            context.Fail();
        }
    }
}
