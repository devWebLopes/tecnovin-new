using Empresa.Api.DTOs;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints de autenticação — login, refresh token, alteração de senha
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registra os endpoints de autenticação no pipeline
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/auth")
            .WithTags("Autenticação");

        // POST /api/v1/auth/login
        grupo.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithSummary("Realiza o login do usuário")
            .WithDescription("Valida credenciais (login + senha) e retorna JWT + refresh token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        // POST /api/v1/auth/refresh
        grupo.MapPost("/refresh", RefreshTokenAsync)
            .AllowAnonymous()
            .WithSummary("Renova o access token")
            .WithDescription("Gera um novo access token a partir do refresh token enviado no body")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status501NotImplemented);

        // POST /api/v1/auth/alterar-senha
        grupo.MapPost("/alterar-senha", AlterarSenhaAsync)
            .RequireAuthorization()
            .WithSummary("Altera a senha do usuário")
            .WithDescription("Altera a senha validando a senha antiga. Requer autenticação.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthService authService)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Json(
                new { error = "Unauthorized", message = ex.Message },
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        IAuthService authService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Results.BadRequest(new { error = "RefreshToken é obrigatório" });

            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            return Results.Ok(response);
        }
        catch (NotImplementedException)
        {
            return Results.Problem(
                detail: "Refresh token ainda não implementado",
                statusCode: StatusCodes.Status501NotImplemented);
        }
    }

    private static async Task<IResult> AlterarSenhaAsync(
        AlterarSenhaRequest request,
        IAuthService authService)
    {
        try
        {
            await authService.AlterarSenhaAsync(request.IdUsuario, request.SenhaAntiga, request.SenhaNova);
            return Results.NoContent();
        }
        catch (NotImplementedException)
        {
            return Results.Problem(
                detail: "Alteração de senha ainda não implementada",
                statusCode: StatusCodes.Status501NotImplemented);
        }
    }
}

/// <summary>
/// DTO para refresh token
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO para alteração de senha
/// </summary>
public class AlterarSenhaRequest
{
    public int IdUsuario { get; set; }
    public string SenhaAntiga { get; set; } = string.Empty;
    public string SenhaNova { get; set; } = string.Empty;
}
