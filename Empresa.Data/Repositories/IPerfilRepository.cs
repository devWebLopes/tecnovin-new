using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de perfis — CRUD + vinculação de páginas
/// </summary>
public interface IPerfilRepository
{
    Task<IEnumerable<Perfil>> GetAllAsync(string? search = null);
    Task<Perfil?> GetByIdAsync(int id);
    Task<int> CreateAsync(Perfil perfil);
    Task<bool> UpdateAsync(Perfil perfil);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PaginaTree>> GetPaginasTreeAsync(int idPerfil);
    Task SalvarPermissoesAsync(int idPerfil, List<int> vincularIds, List<int> desvincularIds);
}

/// <summary>
/// Modelo auxiliar para árvore hierárquica de páginas com vínculo
/// </summary>
public class PaginaTree
{
    public int IDPagina { get; set; }
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IDPaginaPai { get; set; }
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public string Ativo { get; set; } = "S";
    public int Nivel { get; set; }
    public string Vinculado { get; set; } = "N";
}