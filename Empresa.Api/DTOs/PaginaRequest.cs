namespace Empresa.Api.DTOs;

/// <summary>
/// DTO para criação/atualização de página
/// </summary>
public class PaginaRequest
{
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IdPaginaPai { get; set; }
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}