using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class PerfilServiceTests
{
    private readonly Mock<IPerfilRepository> _mockRepo;
    private readonly Mock<IMenuService> _mockMenuService;
    private readonly PerfilService _service;

    public PerfilServiceTests()
    {
        _mockRepo = new Mock<IPerfilRepository>();
        _mockMenuService = new Mock<IMenuService>();
        var logger = Mock.Of<ILogger<PerfilService>>();
        _service = new PerfilService(_mockRepo.Object, _mockMenuService.Object, logger);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResponses()
    {
        var perfis = new List<Perfil>
        {
            new() { IDPerfil = 1, Descricao = "Administrador" },
            new() { IDPerfil = 2, Descricao = "Operador" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(null)).ReturnsAsync(perfis);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Equal("Administrador", result.First().Descricao);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedResponse()
    {
        var request = new PerfilRequest { Descricao = "Novo Perfil" };
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Perfil>()))
            .ReturnsAsync(10);

        var result = await _service.CreateAsync(request);

        Assert.Equal(10, result.IdPerfil);
        Assert.Equal("Novo Perfil", result.Descricao);
    }

    [Fact]
    public async Task CreateAsync_EmptyDescricao_ThrowsValidationException()
    {
        var request = new PerfilRequest { Descricao = "" };
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ExistingPerfil_ReturnsTrue()
    {
        var request = new PerfilRequest { Descricao = "Editado" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Perfil { IDPerfil = 1, Descricao = "Original" });
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Perfil>())).ReturnsAsync(true);
        var result = await _service.UpdateAsync(1, request);
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingPerfil_ThrowsNotFoundException()
    {
        var request = new PerfilRequest { Descricao = "Editado" };
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Perfil?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(99, request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingPerfil_ReturnsTrue()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Perfil { IDPerfil = 1, Descricao = "Teste" });
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _service.DeleteAsync(1);
        Assert.True(result);
    }

    [Fact]
    public async Task GetPaginasTreeAsync_ReturnsHierarchy()
    {
        var paginas = new List<PaginaTree>
        {
            new() { IDPagina = 1, TituloMenu = "Acesso", IDPaginaPai = null, Ordem = 1, Ativo = "S", Vinculado = "N" },
            new() { IDPagina = 2, TituloMenu = "Perfil", IDPaginaPai = 1, Ordem = 1, Ativo = "S", Vinculado = "S" }
        };
        _mockRepo.Setup(r => r.GetPaginasTreeAsync(1)).ReturnsAsync(paginas);
        var result = await _service.GetPaginasTreeAsync(1);
        Assert.Single(result);
        Assert.Single(result.First().Filhos);
        Assert.True(result.First().Filhos[0].Vinculado);
    }
}