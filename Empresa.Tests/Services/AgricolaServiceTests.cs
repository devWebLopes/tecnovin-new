using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Moq;

namespace Empresa.Tests.Services;

public class AgricolaServiceTests
{
    private readonly Mock<IAgricolaRepository> _mockRepo;
    private readonly Mock<IAcessoRepository> _mockAcesso;
    private readonly AgricolaService _service;
    private const int IdPerfil = 10;

    public AgricolaServiceTests()
    {
        _mockRepo = new Mock<IAgricolaRepository>();
        _mockAcesso = new Mock<IAcessoRepository>();
        _service = new AgricolaService(_mockRepo.Object, _mockAcesso.Object);
    }

    private void SetupPermissao(bool permitido = true)
    {
        _mockAcesso.Setup(r => r.GetPaginaByChaveAsync("comprasFrutasPorEmpresas", IdPerfil))
            .ReturnsAsync(permitido ? new Pagina { IDPagina = 31, ChaveControle = "comprasFrutasPorEmpresas" } : null);
    }

    [Fact]
    public async Task GetRecebimentoFrutasAsync_WithPermission_ReturnsGridDinamica()
    {
        SetupPermissao(true);
        var colunas = new List<string> { "EMPRESA", "LINHA", "QTDE_D0" };
        var linhas = new List<Dictionary<string, object>> { new() { ["EMPRESA"] = "TECNOVIN", ["LINHA"] = "UVAS", ["QTDE_D0"] = 1500 } };
        _mockRepo.Setup(r => r.GetRecebimentoFrutasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((colunas.AsReadOnly(), linhas.Cast<IDictionary<string, object>>()));

        var result = await _service.GetRecebimentoFrutasAsync(DateTime.Today, IdPerfil);

        Assert.Equal(3, result.Colunas.Count);
        Assert.NotEmpty(result.UltimaAtualizacao);
        Assert.NotEmpty(result.DataReferencia);
    }

    [Fact]
    public async Task GetRecebimentoFrutasAsync_WithoutPermission_ThrowsUnauthorized()
    {
        SetupPermissao(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetRecebimentoFrutasAsync(DateTime.Today, IdPerfil));
    }

    [Fact]
    public async Task GetDetalhamentoAsync_InvalidColunaClicada_ThrowsArgumentException()
    {
        SetupPermissao(true);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetDetalhamentoAsync(DateTime.Today, "EMP", "UVAS", "RS", "INVALIDA", IdPerfil));
        Assert.Contains("Coluna clicada inválida", ex.Message);
    }

    [Fact]
    public async Task GetDetalhamentoAsync_ValidColunaClicada_ReturnsGrid()
    {
        SetupPermissao(true);
        var colunas = new List<string> { "VARIEDADE", "QTDE" };
        var linhas = new List<Dictionary<string, object>> { new() { ["VARIEDADE"] = "TO005", ["QTDE"] = 500 } };
        _mockRepo.Setup(r => r.GetRecebimentoFrutasDetAsync(It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0"))
            .ReturnsAsync((colunas.AsReadOnly(), linhas.Cast<IDictionary<string, object>>()));

        var result = await _service.GetDetalhamentoAsync(DateTime.Today, "EMP", "UVAS", "RS", "QTDE_D0", IdPerfil);

        Assert.Equal(2, result.Colunas.Count);
    }

    [Fact]
    public async Task GetNotasFiscaisAsync_CalculaCdVariedade_Correctly()
    {
        SetupPermissao(true);
        var colunas = new List<string> { "NR_NOTAFISCAL" };
        var linhas = new List<Dictionary<string, object>> { new() { ["NR_NOTAFISCAL"] = "12345" } };
        _mockRepo.Setup(r => r.GetRecebimentoFrutasDetNfAsync(
                It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, null))
            .ReturnsAsync((colunas.AsReadOnly(), linhas.Cast<IDictionary<string, object>>()));

        var result = await _service.GetNotasFiscaisAsync(
            DateTime.Today, "EMP", "UVAS", "RS", "QTDE_D0", null, "TO005 - Italia", IdPerfil);

        Assert.Single(result.Colunas);
        _mockRepo.Verify(r => r.GetRecebimentoFrutasDetNfAsync(
            It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, null), Times.Once);
    }

    [Fact]
    public async Task GetNotasFiscaisAsync_CdVariedade_NI_ReturnsNI()
    {
        SetupPermissao(true);
        var colunas = new List<string> { "NR_NOTAFISCAL" };
        var linhas = new List<Dictionary<string, object>>();
        _mockRepo.Setup(r => r.GetRecebimentoFrutasDetNfAsync(
                It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, "NI"))
            .ReturnsAsync((colunas.AsReadOnly(), linhas.Cast<IDictionary<string, object>>()));

        await _service.GetNotasFiscaisAsync(
            DateTime.Today, "EMP", "UVAS", "RS", "QTDE_D0", null, "NI001 - Niágara", IdPerfil);

        _mockRepo.Verify(r => r.GetRecebimentoFrutasDetNfAsync(
            It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, "NI"), Times.Once);
    }

    [Fact]
    public async Task GetNotasFiscaisAsync_CdVariedade_Null_ReturnsNull()
    {
        SetupPermissao(true);
        var colunas = new List<string> { "NR_NOTAFISCAL" };
        var linhas = new List<Dictionary<string, object>>();
        _mockRepo.Setup(r => r.GetRecebimentoFrutasDetNfAsync(
                It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, null))
            .ReturnsAsync((colunas.AsReadOnly(), linhas.Cast<IDictionary<string, object>>()));

        await _service.GetNotasFiscaisAsync(
            DateTime.Today, "EMP", "UVAS", "RS", "QTDE_D0", null, null, IdPerfil);

        _mockRepo.Verify(r => r.GetRecebimentoFrutasDetNfAsync(
            It.IsAny<DateTime>(), "EMP", "UVAS", "RS", "QTDE_D0", null, null), Times.Once);
    }

    [Fact]
    public async Task GetNotasFiscaisAsync_InvalidColunaClicada_ThrowsArgumentException()
    {
        SetupPermissao(true);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetNotasFiscaisAsync(DateTime.Today, "EMP", "UVAS", "RS", "INVALIDA", null, null, IdPerfil));
        Assert.Contains("Coluna clicada inválida", ex.Message);
    }
}
