using Empresa.Api.DTOs.Request;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Moq;

namespace Empresa.Tests.Services;

public class MetaCompraServiceTests
{
    private readonly Mock<IMetaCompraRepository> _mockRepo;
    private readonly Mock<IAcessoRepository> _mockAcesso;
    private readonly MetaCompraService _service;
    private const int IdPerfil = 10;

    public MetaCompraServiceTests()
    {
        _mockRepo = new Mock<IMetaCompraRepository>();
        _mockAcesso = new Mock<IAcessoRepository>();
        _service = new MetaCompraService(_mockRepo.Object, _mockAcesso.Object);
    }

    private void SetupPermissao(bool permitido = true)
    {
        _mockAcesso.Setup(r => r.GetPaginaByChaveAsync("cadastroSafraMeta", IdPerfil))
            .ReturnsAsync(permitido ? new Pagina { IDPagina = 30, ChaveControle = "cadastroSafraMeta" } : null);
    }

    private static MetaCompraRequest ValidRequest() => new()
    {
        CdLinha = 10,
        Safra = "2026",
        DtInicial = new DateTime(2026, 1, 1),
        DtFinal = new DateTime(2026, 12, 31),
        MetaQtde = 1500000,
        CdEmpresa = 2,
    };

    [Fact]
    public async Task GetAllAsync_WithPermission_ReturnsMetas()
    {
        SetupPermissao(true);
        var metas = new List<MetaCompra>
        {
            new() { IdMetaCompra = 1, CdLinha = 10, Safra = "2026", DtInicial = new DateTime(2026, 1, 1), DtFinal = new DateTime(2026, 12, 31), MetaQtde = 1500000, CdEmpresa = 2 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(metas);

        var result = await _service.GetAllAsync(IdPerfil);

        Assert.Single(result);
        Assert.Equal("2026", result[0].Safra);
    }

    [Fact]
    public async Task GetAllAsync_WithoutPermission_ThrowsUnauthorized()
    {
        SetupPermissao(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetAllAsync(IdPerfil));
    }

    [Fact]
    public async Task GetEmpresasAsync_WithPermission_ReturnsFixedDomain()
    {
        SetupPermissao(true);

        var result = await _service.GetEmpresasAsync(IdPerfil);

        Assert.Equal(4, result.Count);
        Assert.Equal(2, result[0].CdEmpresa);
        Assert.Equal(700, result[3].CdEmpresa);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedMeta()
    {
        SetupPermissao(true);
        var request = ValidRequest();
        _mockRepo.Setup(r => r.InsertAsync(It.IsAny<MetaCompra>())).ReturnsAsync(100);

        var result = await _service.CreateAsync(request, IdPerfil);

        Assert.Equal(100, result.IdMetaCompra);
        Assert.Equal("2026", result.Safra);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<MetaCompra>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DtFinalMenorDtInicial_ThrowsInvalidOperationException()
    {
        SetupPermissao(true);
        var request = ValidRequest();
        request.DtFinal = new DateTime(2025, 1, 1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, IdPerfil));
        Assert.Equal("A Data Final deve ser maior ou igual à Data Inicial.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_EmpresaForaDominio_ThrowsInvalidOperationException()
    {
        SetupPermissao(true);
        var request = ValidRequest();
        request.CdEmpresa = 999;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, IdPerfil));
        Assert.Equal("Empresa inválida.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMeta_ReturnsTrue()
    {
        SetupPermissao(true);
        var request = ValidRequest();
        var existing = new MetaCompra { IdMetaCompra = 1, CdLinha = 5, Safra = "2025", DtInicial = DateTime.Now, DtFinal = DateTime.Now, MetaQtde = 100, CdEmpresa = 2 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<MetaCompra>())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(1, request, IdPerfil);

        Assert.True(result);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<MetaCompra>(m => m.Safra == "2026")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingMeta_ThrowsKeyNotFoundException()
    {
        SetupPermissao(true);
        var request = ValidRequest();
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MetaCompra?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(999, request, IdPerfil));
    }

    [Fact]
    public async Task DeleteAsync_ExistingMeta_ReturnsTrue()
    {
        SetupPermissao(true);
        var existing = new MetaCompra { IdMetaCompra = 1, CdLinha = 10, Safra = "2026", DtInicial = DateTime.Now, DtFinal = DateTime.Now, MetaQtde = 100, CdEmpresa = 2 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1, IdPerfil);

        Assert.True(result);
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingMeta_ThrowsKeyNotFoundException()
    {
        SetupPermissao(true);
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((MetaCompra?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(999, IdPerfil));
    }
}
