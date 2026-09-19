using Empresa.Api.DTOs;
using Empresa.Api.Services;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Empresa.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAcessoRepository> _mockRepo;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockRepo = new Mock<IAcessoRepository>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-with-at-least-32-characters!",
                ["Jwt:Issuer"] = "Test",
                ["Jwt:Audience"] = "Test",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        _service = new AuthService(_mockRepo.Object, config, Mock.Of<ILogger<AuthService>>());
    }

    [Fact]
    public async Task LoginAsync_InvalidUser_ThrowsUnauthorized()
    {
        _mockRepo.Setup(r => r.GetUsuarioByLoginAsync("invalid")).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequest { Login = "invalid", Senha = "x" }));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("123456");
        var usuario = new Usuario { IDUsuario = 1, Nome = "João", Login = "joao", Senha = hash, IDPerfil = 1, Ativo = "S" };

        _mockRepo.Setup(r => r.GetUsuarioByLoginAsync("joao")).ReturnsAsync(usuario);
        _mockRepo.Setup(r => r.SalvaAcessoUsuarioAsync(1)).Returns(Task.CompletedTask);

        var result = await _service.LoginAsync(new LoginRequest { Login = "joao", Senha = "123456" });

        Assert.NotNull(result);
        Assert.Equal("João", result.Nome);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
    }
}