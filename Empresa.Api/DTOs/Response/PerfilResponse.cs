namespace Empresa.Api.DTOs.Response;

/// <summary>
/// DTO de resposta com dados do perfil
/// </summary>
public class PerfilResponse
{
    public int IdPerfil { get; set; }
    public string Descricao { get; set; } = string.Empty;
}