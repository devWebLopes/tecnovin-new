using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;

namespace Empresa.Api.Services;

public interface IAcessoBiService
{
    /// <summary>
    /// RF02 — Lista usuários com acesso ao B.I. (grid mestre)
    /// </summary>
    Task<IEnumerable<UsuarioBiResponse>> GetUsuariosComAcessoAsync(int idPerfil, string? search = null);

    /// <summary>
    /// RF03 — Lista empresas vinculadas ao usuário (grid detalhe)
    /// </summary>
    Task<IEnumerable<EmpresaBiResponse>> GetEmpresasDoUsuarioAsync(int idPerfil, long idUsuario);

    /// <summary>
    /// RF05 — Lista empresas ainda NÃO vinculadas ao usuário (dropdown)
    /// </summary>
    Task<IEnumerable<EmpresaDisponivelResponse>> GetEmpresasDisponiveisAsync(int idPerfil, long idUsuario);

    /// <summary>
    /// RF04 — Concede acesso total (usuário → todas empresas)
    /// </summary>
    Task<IEnumerable<UsuarioBiResponse>> ConcederAcessoTotalAsync(int idPerfil, ConcederAcessoTotalRequest request);

    /// <summary>
    /// RF05 — Vincula uma empresa específica ao usuário
    /// </summary>
    Task<EmpresaBiResponse> VincularEmpresaAsync(int idPerfil, long idUsuario, VincularEmpresaRequest request);

    /// <summary>
    /// RF06 — Remove um vínculo específico
    /// </summary>
    Task RemoverAcessoAsync(int idPerfil, long idUsuarioEmpresa);
}