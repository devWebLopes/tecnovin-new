namespace Empresa.Data.Repositories;

/// <summary>
/// Resultado do grid mestre — usuário com acesso ao B.I. e agregação das empresas
/// </summary>
public class UsuarioBi
{
    public long IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Empresas { get; set; } = string.Empty;
}

/// <summary>
/// Empresa disponível para vínculo (dropdown do detalhe)
/// </summary>
public class EmpresaDisponivel
{
    public long CdEmpresa { get; set; }
    public string NmFantasia { get; set; } = string.Empty;
}

/// <summary>
/// Verificação de existência — usuário e empresa
/// </summary>
public class ExisteResult
{
    public long Existe { get; set; }
}

/// <summary>
/// Interface do repositório de Acesso B.I. — gerencia a tabela USUARIO_EMPRESA
/// </summary>
public interface IAcessoBiRepository
{
    /// <summary>
    /// Q2 — Lista usuários com acesso ao B.I. (≥1 vínculo), com agregação LISTAGG das empresas
    /// </summary>
    Task<IEnumerable<UsuarioBi>> GetUsuariosComAcessoAsync(string? search = null);

    /// <summary>
    /// Q7 — Lista empresas vinculadas ao usuário (grid detalhe)
    /// </summary>
    Task<IEnumerable<Empresa.Data.Models.UsuarioEmpresa>> GetEmpresasDoUsuarioAsync(long idUsuario);

    /// <summary>
    /// Q6 — Lista empresas ainda NÃO vinculadas ao usuário (dropdown)
    /// </summary>
    Task<IEnumerable<EmpresaDisponivel>> GetEmpresasDisponiveisAsync(long idUsuario);

    /// <summary>
    /// Q3 — Concede acesso total: INSERT em massa (1 linha por empresa da view)
    /// </summary>
    Task ConcederAcessoTotalAsync(long idUsuario);

    /// <summary>
    /// Q8 — Insere 1 vínculo usuário×empresa
    /// </summary>
    Task ConcederAcessoEmpresaAsync(long idUsuario, long codigoEmpresa);

    /// <summary>
    /// Q9 — Remove vínculo por PK (corrige defeito D2: usa ID_USUARIO_EMPRESA real)
    /// </summary>
    Task<bool> RemoverAcessoAsync(long idUsuarioEmpresa);

    /// <summary>
    /// Verifica se o usuário possui pelo menos 1 vínculo em USUARIO_EMPRESA
    /// </summary>
    Task<bool> UsuarioPossuiAcessoAsync(long idUsuario);

    /// <summary>
    /// Verifica se o vínculo usuário×empresa já existe
    /// </summary>
    Task<bool> VinculoJaExisteAsync(long idUsuario, long codigoEmpresa);

    /// <summary>
    /// Verifica se o usuário existe em ACESSO_CADASTRO_USUARIO
    /// </summary>
    Task<bool> UsuarioExisteAsync(long idUsuario);

    /// <summary>
    /// Verifica se a empresa existe em VW_EMPRESA_NEW
    /// </summary>
    Task<bool> EmpresaExisteAsync(long codigoEmpresa);

    /// <summary>
    /// Verifica se o vínculo (PK) existe em USUARIO_EMPRESA
    /// </summary>
    Task<bool> VinculoExisteAsync(long idUsuarioEmpresa);
}