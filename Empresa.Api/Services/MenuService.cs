using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Empresa.Api.Services;

/// <summary>
/// Implementação do serviço de menu — monta a árvore (algoritmo do menus.md §10.3),
/// mantém o cache por perfil (RF07, D-08) e orquestra mais acessados + usuário/B.I.
/// </summary>
public class MenuService : IMenuService
{
    private const string GenerationKey = "menu:tree:generation";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IAcessoRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IAcessoRepository repository, IMemoryCache cache, ILogger<MenuService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MenuResponse> GetMenuAsync(int idUsuario, int idPerfil)
    {
        var tree = await GetOrCreateTreeAsync(idPerfil);
        var maisAcessados = await _repository.GetPaginasMaisAcessadasAsync(idUsuario, idPerfil);
        var usuario = await _repository.GetMenuUsuarioAsync(idUsuario);

        return new MenuResponse
        {
            Menu = tree,
            MaisAcessados = maisAcessados.Select(m => new MaisAcessadoItemResponse
            {
                IdPagina = m.IDPagina,
                ChaveControle = m.ChaveControle,
                TituloMenu = m.TituloMenu,
                TituloAba = m.TituloAba,
                Url = m.Url,
                Quantidade = m.Quantidade
            }).ToList(),
            Usuario = new MenuUsuarioResponse
            {
                Login = usuario?.Login ?? string.Empty,
                Nome = usuario?.Nome ?? string.Empty,
                ExibeLinkBi = (usuario?.Vinculos ?? 0) > 0
            }
        };
    }

    /// <inheritdoc />
    public async Task RegistrarAcessoAsync(string chaveControle, int idUsuario, int idPerfil)
    {
        try
        {
            // RN-12: resolve a chave e valida a permissão ANTES do upsert — acesso não
            // autorizado não é registrado.
            var pagina = await _repository.GetPaginaByChaveAsync(chaveControle, idPerfil);
            if (pagina is null)
            {
                _logger.LogWarning(
                    "Telemetria recusada — chave '{Chave}' sem permissão para o perfil {Perfil}",
                    chaveControle, idPerfil);
                return; // D-15: ao invés de 403, apenas ignora silenciosamente
            }

            // RN-08: upsert (MERGE) com efeito idêntico ao SQL 5.3 do legado.
            await _repository.RegistraAcessoPaginaAsync(idUsuario, pagina.IDPagina);
            _logger.LogInformation(
                "Acesso registrado — usuário {Usuario}, página {Pagina} ({Chave})",
                idUsuario, pagina.IDPagina, chaveControle);
        }
        catch (Exception ex)
        {
            // RF03.5/D-03: falha de telemetria NUNCA interrompe a navegação.
            // Apenas loga o erro e retorna sucesso (204) para o frontend.
            _logger.LogWarning(ex,
                "Falha ao registrar telemetria — usuário {Usuario}, chave '{Chave}'",
                idUsuario, chaveControle);
        }
    }

    /// <inheritdoc />
    public void Invalidate(int? idPerfil = null)
    {
        try
        {
            if (idPerfil.HasValue)
            {
                _cache.Remove(TreeCacheKey(idPerfil.Value));
                _logger.LogInformation("Cache do menu invalidado para o perfil {Perfil}", idPerfil.Value);
                return;
            }

            // Invalidação total: avança a geração — todas as chaves menu:tree:{gen}:{perfil}
            // ficam obsoletas (IMemoryCache não permite enumerar chaves).
            var geracao = GetGeneration();
            _cache.Set(GenerationKey, geracao + 1, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            });
            _logger.LogInformation("Cache do menu invalidado globalmente (geração {Geracao} → {Nova})",
                geracao, geracao + 1);
        }
        catch (Exception ex)
        {
            // RF07.4 — falha de cache nunca quebra o fluxo.
            _logger.LogWarning(ex, "Falha ao invalidar o cache do menu");
        }
    }

    /// <summary>
    /// Monta (ou recupera do cache) a árvore de menu do perfil.
    /// Falha de banco PROPAGA 500 (RF01.5/D-03) — nunca retorna menu vazio silencioso.
    /// </summary>
    private async Task<List<MenuItemResponse>> GetOrCreateTreeAsync(int idPerfil)
    {
        var cacheKey = TreeCacheKey(idPerfil);

        if (_cache.TryGetValue(cacheKey, out List<MenuItemResponse>? cached) && cached is not null)
            return cached;

        try
        {
            var paginas = await _repository.GetPaginasMenuAsync(idPerfil);
            var tree = BuildTree(paginas.ToList());

            _cache.Set(cacheKey, tree, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            });

            return tree;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao montar o menu do perfil {Perfil}", idPerfil);
            throw;
        }
    }

    /// <summary>
    /// Chave de cache inclui a geração para suportar invalidação global (RF07.3).
    /// </summary>
    private string TreeCacheKey(int idPerfil) => $"menu:tree:{GetGeneration()}:{idPerfil}";

    private int GetGeneration()
    {
        return _cache.GetOrCreate(GenerationKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return 0;
        });
    }

    /// <summary>
    /// Algoritmo fiel ao legado (menus.md §10.3): raízes = pais nulos (ou pais ausentes da
    /// lista, ex.: ancestral inativo excluído pelo filtro ATIVO='S' — D-06), ordenadas por
    /// Ordem; filhos recursivos ordenados por Ordem em cada nível (RN-04/RN-05).
    /// </summary>
    private static List<MenuItemResponse> BuildTree(List<Pagina> flatList)
    {
        var pageIds = new HashSet<int>(flatList.Select(p => p.IDPagina));

        var raizes = flatList
            .Where(p => p.IDPaginaPai is null || !pageIds.Contains(p.IDPaginaPai.Value))
            .OrderBy(p => p.Ordem)
            .ToList();

        var result = new List<MenuItemResponse>();
        foreach (var raiz in raizes)
        {
            var no = MapToMenuItem(raiz);
            PreencherFilhos(flatList, no, raiz.IDPagina);
            result.Add(no);
        }

        return result;
    }

    private static void PreencherFilhos(List<Pagina> flatList, MenuItemResponse noPai, int idPai)
    {
        var filhos = flatList
            .Where(p => p.IDPaginaPai == idPai)
            .OrderBy(p => p.Ordem)
            .ToList();

        foreach (var filho in filhos)
        {
            var noFilho = MapToMenuItem(filho);
            PreencherFilhos(flatList, noFilho, filho.IDPagina);
            noPai.Filhos.Add(noFilho);
        }
    }

    private static MenuItemResponse MapToMenuItem(Pagina p)
    {
        return new MenuItemResponse
        {
            IdPagina = p.IDPagina,
            ChaveControle = p.ChaveControle,
            TituloMenu = p.TituloMenu,
            TituloAba = p.TituloAba,
            Url = p.Url,
            ToolTip = p.ToolTip,
            Ordem = p.Ordem
        };
    }
}
