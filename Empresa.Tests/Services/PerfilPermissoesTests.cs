using Empresa.Api.DTOs.Request;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class PerfilPermissoesTests
{
    private readonly Mock<IPerfilRepository> _mockRepo;
    private readonly Mock<IMenuService> _mockMenuService;
    private readonly PerfilService _service;

    public PerfilPermissoesTests()
    {
        _mockRepo = new Mock<IPerfilRepository>();
        _mockMenuService = new Mock<IMenuService>();
        var logger = Mock.Of<ILogger<PerfilService>>();
        _service = new PerfilService(_mockRepo.Object, _mockMenuService.Object, logger);
    }

    [Fact]
    public async Task SalvarPermissoesAsync_ValidPerfil_CompletesSuccessfully()
    {
        var request = new PerfilPaginasRequest
        {
            VincularIds = new List<int> { 1, 2, 3 },
            DesvincularIds = new List<int> { 4, 5 }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Perfil { IDPerfil = 1, Descricao = "Teste" });
        _mockRepo.Setup(r => r.SalvarPermissoesAsync(1, It.IsAny<List<int>>(), It.IsAny<List<int>>()))
            .Returns(Task.CompletedTask);

        await _service.SalvarPermissoesAsync(1, request);

        _mockRepo.Verify(r => r.SalvarPermissoesAsync(1,
            It.Is<List<int>>(v => v.Count == 3),
            It.Is<List<int>>(v => v.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task SalvarPermissoesAsync_NonExistingPerfil_ThrowsNotFoundException()
    {
        var request = new PerfilPaginasRequest();
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Perfil?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.SalvarPermissoesAsync(99, request));
    }
}