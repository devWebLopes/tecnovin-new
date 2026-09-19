namespace Empresa.Api.DTOs.Response;

/// <summary>
/// Dados do usuário na barra do menu — login, nome e link condicional do B.I. (RN-09, RN-14).
/// </summary>
public class MenuUsuarioResponse
{
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool ExibeLinkBi { get; set; }
}
