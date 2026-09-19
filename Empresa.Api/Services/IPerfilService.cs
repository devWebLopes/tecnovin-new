using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;

namespace Empresa.Api.Services;

public interface IPerfilService
{
    Task<IEnumerable<PerfilResponse>> GetAllAsync(string? search = null);
    Task<PerfilResponse?> GetByIdAsync(int id);
    Task<PerfilResponse> CreateAsync(PerfilRequest request);
    Task<bool> UpdateAsync(int id, PerfilRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PaginaTreeResponse>> GetPaginasTreeAsync(int idPerfil);
    Task SalvarPermissoesAsync(int idPerfil, PerfilPaginasRequest request);
}