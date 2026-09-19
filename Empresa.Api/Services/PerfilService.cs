using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _repository;
    private readonly IMenuService _menuService;
    private readonly ILogger<PerfilService> _logger;

    public PerfilService(IPerfilRepository repository, IMenuService menuService, ILogger<PerfilService> logger)
    {
        _repository = repository;
        _menuService = menuService;
        _logger = logger;
    }

    public async Task<IEnumerable<PerfilResponse>> GetAllAsync(string? search = null)
    {
        var perfis = await _repository.GetAllAsync(search);
        return perfis.Select(MapToResponse);
    }

    public async Task<PerfilResponse?> GetByIdAsync(int id)
    {
        var perfil = await _repository.GetByIdAsync(id);
        return perfil is null ? null : MapToResponse(perfil);
    }

    public async Task<PerfilResponse> CreateAsync(PerfilRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao))
            throw new ValidationException("Descrição é obrigatória");

        var perfil = new Perfil
        {
            Descricao = request.Descricao
        };

        var id = await _repository.CreateAsync(perfil);
        perfil.IDPerfil = id;

        _logger.LogInformation("Perfil {Descricao} (ID: {Id}) criado com sucesso", perfil.Descricao, id);
        return MapToResponse(perfil);
    }

    public async Task<bool> UpdateAsync(int id, PerfilRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao))
            throw new ValidationException("Descrição é obrigatória");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Perfil", id);

        existing.Descricao = request.Descricao;

        return await _repository.UpdateAsync(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Verificar se perfil existe
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Perfil", id);

        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PaginaTreeResponse>> GetPaginasTreeAsync(int idPerfil)
    {
        var paginas = (await _repository.GetPaginasTreeAsync(idPerfil)).ToList();

        // Identifica nós raiz: IDPaginaPai é null, 0 ou aponta para ID inexistente na lista
        var pageIds = new HashSet<int>(paginas.Select(p => p.IDPagina));
        var pais = paginas
            .Where(p => p.IDPaginaPai is null || p.IDPaginaPai == 0 || !pageIds.Contains(p.IDPaginaPai.Value))
            .OrderBy(p => p.Ordem)
            .ToList();

        var visited = new HashSet<int>();
        return pais
            .Select(pai => MapToTreeResponse(pai, paginas, visited))
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();
    }

    public async Task SalvarPermissoesAsync(int idPerfil, PerfilPaginasRequest request)
    {
        var existing = await _repository.GetByIdAsync(idPerfil);
        if (existing is null)
            throw new NotFoundException("Perfil", idPerfil);

        await _repository.SalvarPermissoesAsync(idPerfil, request.VincularIds, request.DesvincularIds);
        _logger.LogInformation("Permissões do perfil ID {Id} atualizadas", idPerfil);

        // RF07.3/D-08 — invalida o cache do menu desse perfil
        _menuService.Invalidate(idPerfil);
    }

    private static PaginaTreeResponse? MapToTreeResponse(PaginaTree pagina, List<PaginaTree> flatList, HashSet<int> visited)
    {
        if (visited.Contains(pagina.IDPagina))
            return null;

        visited.Add(pagina.IDPagina);

        var filhos = flatList
            .Where(p => p.IDPaginaPai == pagina.IDPagina)
            .OrderBy(p => p.Ordem)
            .Select(filho => MapToTreeResponse(filho, flatList, new HashSet<int>(visited)))
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();

        return new PaginaTreeResponse
        {
            IdPagina = pagina.IDPagina,
            Url = pagina.Url,
            TituloAba = pagina.TituloAba,
            ChaveControle = pagina.ChaveControle,
            TituloMenu = pagina.TituloMenu,
            IdPaginaPai = pagina.IDPaginaPai,
            Ordem = pagina.Ordem,
            ToolTip = pagina.ToolTip,
            Ativo = pagina.Ativo == "S",
            Vinculado = pagina.Vinculado == "S",
            Filhos = filhos
        };
    }

    private static PerfilResponse MapToResponse(Perfil perfil)
    {
        return new PerfilResponse
        {
            IdPerfil = perfil.IDPerfil,
            Descricao = perfil.Descricao
        };
    }
}