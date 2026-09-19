using Empresa.Api.DTOs;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        var logger = Mock.Of<ILogger<UsuarioService>>();
        _service = new UsuarioService(_mockRepo.Object, logger);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResponses()
    {
        var usuarios = new List<Usuario>
        {
            new() { IDUsuario = 1, Nome = "João", Login = "joao", IDPerfil = 1, Ativo = "S", DescricaoPerfil = "Admin" },
            new() { IDUsuario = 2, Nome = "Maria", Login = "maria", IDPerfil = 2, Ativo = "S", DescricaoPerfil = "Operador" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(usuarios);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Equal("João", result.First().Nome);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsResponse()
    {
        var usuario = new Usuario { IDUsuario = 1, Nome = "João", Login = "joao", IDPerfil = 1, Ativo = "S" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("João", result!.Nome);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Usuario?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_DuplicateLogin_ThrowsBusinessException()
    {
        var request = new UsuarioRequest { Login = "joao", Nome = "João", IdPerfil = 1, Senha = "123456" };
        _mockRepo.Setup(r => r.LoginExistsAsync("joao")).ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrue()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Usuario { IDUsuario = 1, Ativo = "S" });
        _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task CreateAsync_MissingSenha_ThrowsValidationException()
    {
        var request = new UsuarioRequest { Login = "joao", Nome = "João", IdPerfil = 1 };
        _mockRepo.Setup(r => r.LoginExistsAsync("joao")).ReturnsAsync(false);

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));
    }
}
