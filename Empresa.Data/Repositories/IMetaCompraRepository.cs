using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

public interface IMetaCompraRepository
{
    Task<IEnumerable<MetaCompra>> GetAllAsync();
    Task<MetaCompra?> GetByIdAsync(long id);
    Task<long> InsertAsync(MetaCompra meta);
    Task<bool> UpdateAsync(MetaCompra meta);
    Task<bool> DeleteAsync(long id);
}
