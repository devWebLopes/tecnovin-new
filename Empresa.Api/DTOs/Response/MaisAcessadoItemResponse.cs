namespace Empresa.Api.DTOs.Response;

/// <summary>
/// Item do painel "Mais Acessados" — top 10 do usuário filtrado por perfil (RN-07, D-04, D-05).
/// </summary>
public class MaisAcessadoItemResponse
{
    public int IdPagina { get; set; }
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
