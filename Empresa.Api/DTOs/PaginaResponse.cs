namespace Empresa.Api.DTOs;

/// <summary>
/// DTO de resposta com dados da página/menu
/// </summary>
public class PaginaResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IdPaginaPai { get; set; }
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}