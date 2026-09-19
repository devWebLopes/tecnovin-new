using Empresa.Api.DTOs;
using Empresa.Api.Middleware;
using Empresa.Data.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Empresa.Api.Services;

/// <summary>
/// Serviço de autenticação com geração de JWT e validação BCrypt
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAcessoRepository _acessoRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAcessoRepository acessoRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _acessoRepository = acessoRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Busca usuário por login para validar existência e senha
        var usuario = await _acessoRepository.GetUsuarioByLoginAsync(request.Login);

        if (usuario is null)
        {
            _logger.LogWarning("Tentativa de login com usuário inexistente: {Login}", request.Login);
            throw new UnauthorizedAccessException("Usuário ou senha inválidos");
        }

        // ATENÇÃO: O banco legado armazena senhas em texto puro.
        // Quando migrar as senhas para BCrypt, substituir por:
        // if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
        if (string.IsNullOrEmpty(usuario.Senha) || usuario.Senha != request.Senha)
        {
            _logger.LogWarning("Tentativa de login com senha inválida: {Login}", request.Login);
            throw new UnauthorizedAccessException("Usuário ou senha inválidos");
        }

        // Registra acesso
        await _acessoRepository.SalvaAcessoUsuarioAsync(usuario.IDUsuario);

        // Gera tokens
        var token = GenerateJwtToken(usuario);
        var refreshToken = GenerateRefreshToken();

        _logger.LogInformation("Login bem-sucedido: {Login} (ID: {Id})", usuario.Login, usuario.IDUsuario);

        return new LoginResponse
        {
            IdUsuario = usuario.IDUsuario,
            Nome = usuario.Nome,
            Login = usuario.Login,
            IdPerfil = usuario.IDPerfil,
            Token = token,
            RefreshToken = refreshToken,
            AtualizaSenha = usuario.AtualizaSenha == "S",
            ExpiraEm = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60))
        };
    }

    public Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implementar validação de refresh token armazenado
        throw new NotImplementedException("Refresh token será implementado na Fase 3");
    }

    public Task AlterarSenhaAsync(int idUsuario, string senhaAntiga, string senhaNova)
    {
        // TODO: Implementar alteração de senha com validação BCrypt
        throw new NotImplementedException("Alterar senha será implementado na Fase 3");
    }

    private string GenerateJwtToken(Data.Models.Usuario usuario)
    {
        var jwtSecret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret não configurado");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("ID_USUARIO", usuario.IDUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Login),
            new Claim("ID_PERFIL", usuario.IDPerfil.ToString()),
            new Claim(ClaimTypes.Role, usuario.IDPerfil.ToString())
        };

        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}