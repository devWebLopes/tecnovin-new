using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de usuários — CRUD com exclusão lógica e vínculos com estabelecimentos
/// </summary>
public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> GetAllAsync(string? search = null, string? ativo = null);
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByLoginAsync(string login);
    Task<int> CreateAsync(Usuario usuario);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int id);
    Task<bool> LoginExistsAsync(string login);
    Task<IEnumerable<UsuarioEstabelecimentoTree>> GetEstabelecimentosTreeAsync(int idUsuario);
    Task SincronizarEstabelecimentosAsync(int idUsuario, List<VinculoEstabelecimento> vincular, List<VinculoEstabelecimento> desvincular);
}

/// <summary>
/// Modelo auxiliar para árvore de estabelecimentos com vínculo
/// </summary>
public class UsuarioEstabelecimentoTree
{
    public int CdEmpresa { get; set; }
    public int CdEstabelecimento { get; set; }
    public string Descritivo { get; set; } = string.Empty;
    public string Vinculado { get; set; } = "N";
}

/// <summary>
/// Modelo para vínculo de estabelecimento
/// </summary>
public class VinculoEstabelecimento
{
    public int CdEmpresa { get; set; }
    public int CdEstabelecimento { get; set; }
}