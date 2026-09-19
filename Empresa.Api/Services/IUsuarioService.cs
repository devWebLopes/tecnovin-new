using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;

namespace Empresa.Api.Services;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioResponse>> GetAllAsync(string? search = null, string? ativo = null);
    Task<UsuarioResponse?> GetByIdAsync(int id);
    Task<UsuarioResponse> CreateAsync(UsuarioRequest request);
    Task<bool> UpdateAsync(int id, UsuarioRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<EstabelecimentoTreeResponse>> GetEstabelecimentosTreeAsync(int idUsuario);
    Task SincronizarEstabelecimentosAsync(int idUsuario, EstabelecimentoVinculoRequest request);
}