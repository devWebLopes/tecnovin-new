using Empresa.Api.DTOs;

namespace Empresa.Api.Services;

/// <summary>
/// Serviço de autenticação — login, refresh token, alteração de senha
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Realiza o login do usuário, validando credenciais e gerando JWT
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Gera um novo access token a partir do refresh token
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Altera a senha do usuário
    /// </summary>
    Task AlterarSenhaAsync(int idUsuario, string senhaAntiga, string senhaNova);
}