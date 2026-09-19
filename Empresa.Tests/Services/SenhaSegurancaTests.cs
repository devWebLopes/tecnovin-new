using Empresa.Api.DTOs;
using Empresa.Api.Middleware;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class SenhaSegurancaTests
{
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly UsuarioService _service;

    public SenhaSegurancaTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        var logger = Mock.Of<ILogger<UsuarioService>>();
        _service = new UsuarioService(_mockRepo.Object, logger);
    }

    [Fact]
    public async Task UsuarioResponse_DoesNotContainSenhaField()
    {
        var usuario = new Usuario
        {
            IDUsuario = 1,
            Nome = "João",
            Login = "joao",
            IDPerfil = 1,
            Ativo = "S",
            Senha = BCrypt.Net.BCrypt.HashPassword("secreta123")
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        var responseType = result!.GetType();
        var properties = responseType.GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("Senha", properties);
        Assert.DoesNotContain("PasswordHash", properties);
    }

    [Fact]
    public async Task CreateAsync_SenhaMinima6Caracteres_ThrowsValidationException()
    {
        var request = new UsuarioRequest
        {
            Nome = "João",
            Login = "joao",
            Senha = "12345",
            IdPerfil = 1,
            Ativo = true
        };
        _mockRepo.Setup(r => r.LoginExistsAsync("joao")).ReturnsAsync(false);

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task BCryptHash_DifferentFromOriginalPassword()
    {
        var original = "senhaForte123";
        var hash = BCrypt.Net.BCrypt.HashPassword(original);

        Assert.NotEqual(original, hash);
        Assert.True(BCrypt.Net.BCrypt.Verify(original, hash));
    }

    [Fact]
    public async Task GetAllAsync_ResponseNeverContainsSenha()
    {
        var usuarios = new List<Usuario>
        {
            new() { IDUsuario = 1, Nome = "João", Login = "joao", IDPerfil = 1, Ativo = "S", Senha = "hash" },
            new() { IDUsuario = 2, Nome = "Maria", Login = "maria", IDPerfil = 2, Ativo = "S", Senha = "hash2" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(usuarios);

        var result = await _service.GetAllAsync();
        var firstResponse = result.First();

        var responseType = firstResponse.GetType();
        var properties = responseType.GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("Senha", properties);
    }
}