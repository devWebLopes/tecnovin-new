using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de páginas — menu hierárquico e acesso
/// </summary>
public interface IPaginaRepository
{
    Task<IEnumerable<Pagina>> GetAllAsync();
    Task<Pagina?> GetByIdAsync(int id);
    Task<int> CreateAsync(Pagina pagina);
    Task<bool> UpdateAsync(Pagina pagina);
    Task<bool> DeleteAsync(int id);
}