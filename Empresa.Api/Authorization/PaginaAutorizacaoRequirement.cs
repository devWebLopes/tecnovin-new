using Microsoft.AspNetCore.Authorization;

namespace Empresa.Api.Authorization;

/// <summary>
/// Requirement da policy "PaginaAcesso" (RN-12) — autoriza quando o perfil do JWT possui
/// vínculo em ACESSO_PERFIL_PAGINA para a CHAVE_CONTROLE da rota.
/// </summary>
public class PaginaAutorizacaoRequirement : IAuthorizationRequirement
{
}
