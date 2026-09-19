namespace Empresa.Api.DTOs;

/// <summary>
/// DTO de resposta para autenticação
/// </summary>
public class LoginResponse
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public int IdPerfil { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool AtualizaSenha { get; set; }
    public DateTime ExpiraEm { get; set; }
}