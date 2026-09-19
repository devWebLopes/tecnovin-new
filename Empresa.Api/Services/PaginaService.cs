using Empresa.Api.DTOs;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public class PaginaService : IPaginaService
{
    private readonly IPaginaRepository _repository;
    private readonly IAcessoRepository _acessoRepository;
    private readonly IMenuService _menuService;
    private readonly ILogger<PaginaService> _logger;

    public PaginaService(
        IPaginaRepository repository,
        IAcessoRepository acessoRepository,
        IMenuService menuService,
        ILogger<PaginaService> logger)
    {
        _repository = repository;
        _acessoRepository = acessoRepository;
        _menuService = menuService;
        _logger = logger;
    }

    public async Task<IEnumerable<PaginaResponse>> GetAllAsync()
    {
        var paginas = await _repository.GetAllAsync();
        return paginas.Select(MapToResponse);
    }

    public async Task<PaginaResponse?> GetByIdAsync(int id)
    {
        var pagina = await _repository.GetByIdAsync(id);
        return pagina is null ? null : MapToResponse(pagina);
    }

    public async Task<PaginaResponse> CreateAsync(PaginaRequest request)
    {
        var pagina = new Pagina
        {
            Url = request.Url,
            TituloAba = request.TituloAba,
            ChaveControle = request.ChaveControle,
            TituloMenu = request.TituloMenu,
            IDPaginaPai = request.IdPaginaPai,
            Ordem = request.Ordem,
            ToolTip = request.ToolTip,
            Ativo = request.Ativo ? "S" : "N"
        };

        var id = await _repository.CreateAsync(pagina);
        pagina.IDPagina = id;

        _logger.LogInformation("Página {Titulo} (ID: {Id}) criada com sucesso", pagina.TituloMenu, id);
        _menuService.Invalidate();
        return MapToResponse(pagina);
    }

    public async Task<bool> UpdateAsync(int id, PaginaRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Página", id);

        existing.Url = request.Url;
        existing.TituloAba = request.TituloAba;
        existing.ChaveControle = request.ChaveControle;
        existing.TituloMenu = request.TituloMenu;
        existing.IDPaginaPai = request.IdPaginaPai;
        existing.Ordem = request.Ordem;
        existing.ToolTip = request.ToolTip;
        existing.Ativo = request.Ativo ? "S" : "N";

        var atualizado = await _repository.UpdateAsync(existing);
        if (atualizado)
            _menuService.Invalidate();
        return atualizado;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var excluido = await _repository.DeleteAsync(id);
        if (excluido)
            _menuService.Invalidate();
        return excluido;
    }

    public async Task<IEnumerable<PaginaTreeResponse>> GetMenuHierarquicoAsync(int idPerfil)
    {
        var paginas = await _acessoRepository.GetPaginasMenuAsync(idPerfil);
        var flatList = paginas.ToList();
        var pais = flatList.Where(p => p.IDPaginaPai is null).OrderBy(p => p.Ordem).ToList();

        return pais.Select(pai => MapToTreeResponse(pai, flatList));
    }

    private static PaginaTreeResponse MapToTreeResponse(Pagina pagina, List<Pagina> flatList)
    {
        var filhos = flatList
            .Where(p => p.IDPaginaPai == pagina.IDPagina)
            .OrderBy(p => p.Ordem)
            .Select(filho => MapToTreeResponse(filho, flatList))
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
            Vinculado = true,
            Filhos = filhos
        };
    }

    private static PaginaResponse MapToResponse(Pagina pagina)
    {
        return new PaginaResponse
        {
            Id = pagina.IDPagina,
            Url = pagina.Url,
            TituloAba = pagina.TituloAba,
            ChaveControle = pagina.ChaveControle,
            TituloMenu = pagina.TituloMenu,
            IdPaginaPai = pagina.IDPaginaPai,
            Ordem = pagina.Ordem,
            ToolTip = pagina.ToolTip,
            Ativo = pagina.Ativo == "S"
        };
    }
}