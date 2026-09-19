using Empresa.Api.Authorization;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Empresa.Tests.Authorization;

public class PaginaAutorizacaoHandlerTests
{
    private readonly Mock<IAcessoRepository> _mockRepo;
    private readonly PaginaAutorizacaoHandler _handler;

    public PaginaAutorizacaoHandlerTests()
    {
        _mockRepo = new Mock<IAcessoRepository>();
        var logger = Mock.Of<ILogger<PaginaAutorizacaoHandler>>();
        _handler = new PaginaAutorizacaoHandler(_mockRepo.Object, logger);
    }

    private static ClaimsPrincipal CriarUsuario(int? idPerfil)
    {
        var identity = new ClaimsIdentity("teste");
        if (idPerfil.HasValue)
            identity.AddClaim(new Claim("ID_PERFIL", idPerfil.Value.ToString()));
        return new ClaimsPrincipal(identity);
    }

    private static HttpContext CriarHttpContext(string chaveControle)
    {
        var http = new DefaultHttpContext();
        http.Request.RouteValues["chaveControle"] = chaveControle;
        return http;
    }

    private async Task<AuthorizationHandlerContext> ExecutarAsync(ClaimsPrincipal user, HttpContext? resource)
    {
        var requirement = new PaginaAutorizacaoRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource);
        await _handler.HandleAsync(context);
        return context;
    }

    [Fact]
    public async Task HandleRequirementAsync_Authorized_Succeeds()
    {
        _mockRepo.Setup(r => r.GetPaginaByChaveAsync("usuarios", 10))
            .ReturnsAsync(new Pagina { IDPagina = 1, ChaveControle = "usuarios" });

        var context = await ExecutarAsync(CriarUsuario(10), CriarHttpContext("usuarios"));

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleRequirementAsync_SemPermissao_Fails()
    {
        _mockRepo.Setup(r => r.GetPaginaByChaveAsync("usuarios", 10))
            .ReturnsAsync((Pagina?)null);

        var context = await ExecutarAsync(CriarUsuario(10), CriarHttpContext("usuarios"));

        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task HandleRequirementAsync_SemClaimPerfil_Fails()
    {
        var context = await ExecutarAsync(CriarUsuario(null), CriarHttpContext("usuarios"));

        Assert.True(context.HasFailed);
        _mockRepo.Verify(r => r.GetPaginaByChaveAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_SemChaveControle_Fails()
    {
        var context = await ExecutarAsync(CriarUsuario(10), new DefaultHttpContext());

        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task HandleRequirementAsync_ChaveNaQueryString_Succeeds()
    {
        _mockRepo.Setup(r => r.GetPaginaByChaveAsync("financeiro", 10))
            .ReturnsAsync(new Pagina { IDPagina = 2, ChaveControle = "financeiro" });

        var http = new DefaultHttpContext();
        http.Request.QueryString = new QueryString("?chaveControle=financeiro");

        var context = await ExecutarAsync(CriarUsuario(10), http);

        Assert.True(context.HasSucceeded);
        _mockRepo.Verify(r => r.GetPaginaByChaveAsync("financeiro", 10), Times.Once);
    }
}
