using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Response;

namespace Empresa.Api.Services;

public interface IPaginaService
{
    Task<IEnumerable<PaginaResponse>> GetAllAsync();
    Task<PaginaResponse?> GetByIdAsync(int id);
    Task<PaginaResponse> CreateAsync(PaginaRequest request);
    Task<bool> UpdateAsync(int id, PaginaRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PaginaTreeResponse>> GetMenuHierarquicoAsync(int idPerfil);
}