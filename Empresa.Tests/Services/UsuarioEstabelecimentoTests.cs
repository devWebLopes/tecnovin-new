using Empresa.Api.DTOs.Request;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class UsuarioEstabelecimentoTests
{
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly UsuarioService _service;

    public UsuarioEstabelecimentoTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        var logger = Mock.Of<ILogger<UsuarioService>>();
        _service = new UsuarioService(_mockRepo.Object, logger);
    }

    [Fact]
    public async Task GetEstabelecimentosTreeAsync_ReturnsGroupedTree()
    {
        var estabs = new List<UsuarioEstabelecimentoTree>
        {
            new() { CdEmpresa = 1, CdEstabelecimento = 1, Descritivo = "TecnoVin - Matriz", Vinculado = "S" },
            new() { CdEmpresa = 1, CdEstabelecimento = 2, Descritivo = "TecnoVin - Filial SP", Vinculado = "N" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Usuario { IDUsuario = 1 });
        _mockRepo.Setup(r => r.GetEstabelecimentosTreeAsync(1)).ReturnsAsync(estabs);

        var result = await _service.GetEstabelecimentosTreeAsync(1);

        Assert.Single(result);
        Assert.Equal(2, result.First().Estabelecimentos.Count);
        Assert.True(result.First().Estabelecimentos[0].Vinculado);
        Assert.False(result.First().Estabelecimentos[1].Vinculado);
    }

    [Fact]
    public async Task SincronizarEstabelecimentosAsync_ValidUsuario_CompletesSuccessfully()
    {
        var request = new EstabelecimentoVinculoRequest
        {
            VincularEstabelecimentos = new List<VinculoEstabelecimentoItem>
            {
                new() { CdEmpresa = 1, CdEstabelecimento = 2 }
            },
            DesvincularEstabelecimentos = new List<VinculoEstabelecimentoItem>
            {
                new() { CdEmpresa = 1, CdEstabelecimento = 3 }
            }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Usuario { IDUsuario = 1 });
        _mockRepo.Setup(r => r.SincronizarEstabelecimentosAsync(1,
            It.IsAny<List<VinculoEstabelecimento>>(),
            It.IsAny<List<VinculoEstabelecimento>>()))
            .Returns(Task.CompletedTask);

        await _service.SincronizarEstabelecimentosAsync(1, request);

        _mockRepo.Verify(r => r.SincronizarEstabelecimentosAsync(1,
            It.Is<List<VinculoEstabelecimento>>(v => v.Count == 1),
            It.Is<List<VinculoEstabelecimento>>(v => v.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task SincronizarEstabelecimentosAsync_NonExistingUsuario_ThrowsNotFoundException()
    {
        var request = new EstabelecimentoVinculoRequest();
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.SincronizarEstabelecimentosAsync(99, request));
    }
}