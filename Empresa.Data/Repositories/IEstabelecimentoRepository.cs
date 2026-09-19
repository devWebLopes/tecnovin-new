using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de estabelecimentos — CRUD com árvore
/// </summary>
public interface IEstabelecimentoRepository
{
    Task<IEnumerable<Estabelecimento>> GetAllAsync();
    Task<Estabelecimento?> GetByIdAsync(int id);
    Task<int> CreateAsync(Estabelecimento estabelecimento);
    Task<bool> UpdateAsync(Estabelecimento estabelecimento);
    Task<bool> DeleteAsync(int id);
}