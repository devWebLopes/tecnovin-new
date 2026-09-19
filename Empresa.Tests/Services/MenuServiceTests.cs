using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class MenuServiceTests
{
    private readonly Mock<IAcessoRepository> _mockRepo;
    private readonly IMemoryCache _cache;
    private readonly MenuService _service;

    public MenuServiceTests()
    {
        _mockRepo = new Mock<IAcessoRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<MenuService>>();
        _service = new MenuService(_mockRepo.Object, _cache, logger);
    }

    // ─── QA-01: árvore — pais implícitos (RN-03), ≥3 níveis (D-07), ordenação (RN-04/RN-05)

    [Fact]
    public async Task GetMenuAsync_BuildsThreeLevelTree_WithParents()
    {
        var paginas = new List<Pagina>
        {
            new() { IDPagina = 1, TituloMenu = "Cadastros", IDPaginaPai = null, Ordem = 1 },
            new() { IDPagina = 2, TituloMenu = "Usuários", IDPaginaPai = 1, Ordem = 1 },
            new() { IDPagina = 3, TituloMenu = "Novo Usuário", IDPaginaPai = 2, Ordem = 1 },
            new() { IDPagina = 4, TituloMenu = "Perfis", IDPaginaPai = 2, Ordem = 2 }
        };
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10)).ReturnsAsync(paginas);
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo { IDUsuario = 5, Nome = "João", Login = "joao", Vinculos = 1 });

        var result = await _service.GetMenuAsync(5, 10);

        var raiz = Assert.Single(result.Menu);
        Assert.Equal("Cadastros", raiz.TituloMenu);
        var nivel2 = Assert.Single(raiz.Filhos);
        Assert.Equal("Usuários", nivel2.TituloMenu);
        Assert.Equal(2, nivel2.Filhos.Count);
        Assert.Equal("Novo Usuário", nivel2.Filhos[0].TituloMenu);
        Assert.Equal("Perfis", nivel2.Filhos[1].TituloMenu);
    }

    [Fact]
    public async Task GetMenuAsync_MissingParent_PromotesToRoot()
    {
        // Pai 99 não está na lista (ex.: ancestral inativo excluído — D-06) → vira raiz.
        var paginas = new List<Pagina>
        {
            new() { IDPagina = 2, TituloMenu = "Órfã", IDPaginaPai = 99, Ordem = 1 },
            new() { IDPagina = 1, TituloMenu = "Cadastros", IDPaginaPai = null, Ordem = 1 }
        };
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10)).ReturnsAsync(paginas);
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        var result = await _service.GetMenuAsync(5, 10);

        Assert.Equal(2, result.Menu.Count);
        Assert.Contains(result.Menu, m => m.TituloMenu == "Órfã");
        Assert.Empty(result.Menu.First(m => m.TituloMenu == "Órfã").Filhos);
    }

    [Fact]
    public async Task GetMenuAsync_SiblingsOrderedByOrdem_AtEachLevel()
    {
        var paginas = new List<Pagina>
        {
            new() { IDPagina = 1, TituloMenu = "Raiz", IDPaginaPai = null, Ordem = 2 },
            new() { IDPagina = 2, TituloMenu = "Início", IDPaginaPai = null, Ordem = 1 },
            new() { IDPagina = 3, TituloMenu = "B", IDPaginaPai = 2, Ordem = 2 },
            new() { IDPagina = 4, TituloMenu = "A", IDPaginaPai = 2, Ordem = 1 }
        };
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10)).ReturnsAsync(paginas);
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        var result = await _service.GetMenuAsync(5, 10);

        Assert.Equal("Início", result.Menu[0].TituloMenu);
        Assert.Equal("Raiz", result.Menu[1].TituloMenu);
        Assert.Equal("A", result.Menu[0].Filhos[0].TituloMenu);
        Assert.Equal("B", result.Menu[0].Filhos[1].TituloMenu);
    }

    // ─── QA-02: cache (RF07)

    [Fact]
    public async Task GetMenuAsync_CachesTree_SecondCallSkipsRepository()
    {
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10))
            .ReturnsAsync(new List<Pagina> { new() { IDPagina = 1, TituloMenu = "Só", IDPaginaPai = null, Ordem = 1 } });
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        await _service.GetMenuAsync(5, 10);
        await _service.GetMenuAsync(5, 10);

        _mockRepo.Verify(r => r.GetPaginasMenuAsync(10), Times.Once);
        _mockRepo.Verify(r => r.GetPaginasMaisAcessadasAsync(5, 10), Times.Exactly(2));
    }

    [Fact]
    public async Task GetMenuAsync_CacheIsolatedByPerfil()
    {
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(It.IsAny<int>()))
            .ReturnsAsync((int p) => new List<Pagina>
            {
                new() { IDPagina = p, TituloMenu = $"Perfil {p}", IDPaginaPai = null, Ordem = 1 }
            });
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        var perfil10 = await _service.GetMenuAsync(5, 10);
        var perfil20 = await _service.GetMenuAsync(5, 20);

        Assert.Equal("Perfil 10", perfil10.Menu[0].TituloMenu);
        Assert.Equal("Perfil 20", perfil20.Menu[0].TituloMenu);
        _mockRepo.Verify(r => r.GetPaginasMenuAsync(10), Times.Once);
        _mockRepo.Verify(r => r.GetPaginasMenuAsync(20), Times.Once);
    }

    [Fact]
    public async Task GetMenuAsync_PerfilInvalidate_ReloadsTree()
    {
        var primeiro = new List<Pagina> { new() { IDPagina = 1, TituloMenu = "Antes", IDPaginaPai = null, Ordem = 1 } };
        var segundo = new List<Pagina> { new() { IDPagina = 1, TituloMenu = "Depois", IDPaginaPai = null, Ordem = 1 } };
        _mockRepo.SetupSequence(r => r.GetPaginasMenuAsync(10))
            .ReturnsAsync(primeiro)
            .ReturnsAsync(segundo);
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        await _service.GetMenuAsync(5, 10);
        _service.Invalidate(10);
        var result = await _service.GetMenuAsync(5, 10);

        Assert.Equal("Depois", result.Menu[0].TituloMenu);
        _mockRepo.Verify(r => r.GetPaginasMenuAsync(10), Times.Exactly(2));
    }

    [Fact]
    public async Task GetMenuAsync_GlobalInvalidate_ReloadsTreeForAllPerfis()
    {
        var primeiro = new List<Pagina> { new() { IDPagina = 1, TituloMenu = "Antes", IDPaginaPai = null, Ordem = 1 } };
        var segundo = new List<Pagina> { new() { IDPagina = 1, TituloMenu = "Depois", IDPaginaPai = null, Ordem = 1 } };
        _mockRepo.SetupSequence(r => r.GetPaginasMenuAsync(10))
            .ReturnsAsync(primeiro)
            .ReturnsAsync(segundo);
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new MenuUsuarioInfo());

        await _service.GetMenuAsync(5, 10);
        _service.Invalidate();
        var result = await _service.GetMenuAsync(5, 10);

        Assert.Equal("Depois", result.Menu[0].TituloMenu);
        _mockRepo.Verify(r => r.GetPaginasMenuAsync(10), Times.Exactly(2));
    }

    [Fact]
    public async Task GetMenuAsync_RepositoryThrows_Propagates500()
    {
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10))
            .ThrowsAsync(new InvalidOperationException("Oracle indisponível"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetMenuAsync(5, 10));
    }

    // ─── QA-01/RN-09: usuário + B.I.

    [Fact]
    public async Task GetMenuAsync_UsuarioComVinculos_ExibeLinkBiTrue()
    {
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10)).ReturnsAsync(Array.Empty<Pagina>());
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(5, 10))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(5))
            .ReturnsAsync(new MenuUsuarioInfo { IDUsuario = 5, Nome = "João", Login = "joao", Vinculos = 3 });

        var result = await _service.GetMenuAsync(5, 10);

        Assert.True(result.Usuario.ExibeLinkBi);
        Assert.Equal("João", result.Usuario.Nome);
    }

    [Fact]
    public async Task GetMenuAsync_UsuarioSemVinculos_ExibeLinkBiFalse()
    {
        _mockRepo.Setup(r => r.GetPaginasMenuAsync(10)).ReturnsAsync(Array.Empty<Pagina>());
        _mockRepo.Setup(r => r.GetPaginasMaisAcessadasAsync(5, 10))
            .ReturnsAsync(Array.Empty<PaginaAcesso>());
        _mockRepo.Setup(r => r.GetMenuUsuarioAsync(5))
            .ReturnsAsync(new MenuUsuarioInfo { IDUsuario = 5, Vinculos = 0 });

        var result = await _service.GetMenuAsync(5, 10);

        Assert.False(result.Usuario.ExibeLinkBi);
    }

    // ─── QA-02/RF03: telemetria (RN-12 autorização ANTES do upsert)

    [Fact]
    public async Task RegistrarAcessoAsync_Authorized_RegistersTelemetry()
    {
        _mockRepo.Setup(r => r.GetPaginaByChaveAsync("usuarios", 10))
            .ReturnsAsync(new Pagina { IDPagina = 42, ChaveControle = "usuarios" });

        await _service.RegistrarAcessoAsync("usuarios", 5, 10);

        _mockRepo.Verify(r => r.RegistraAcessoPaginaAsync(5, 42), Times.Once);
    }

    [Fact]
    public async Task RegistrarAcessoAsync_NotAuthorized_DoesNotRegister()
    {
        _mockRepo.Setup(r => r.GetPaginaByChaveAsync("secret", 10))
            .ReturnsAsync((Pagina?)null);

        await _service.RegistrarAcessoAsync("secret", 5, 10);

        _mockRepo.Verify(r => r.RegistraAcessoPaginaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
