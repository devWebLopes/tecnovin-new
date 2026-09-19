namespace Empresa.Data.Models;

/// <summary>
/// Representa uma página do sistema (menu)
/// </summary>
public class Pagina
{
    public int IDPagina { get; set; }
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IDPaginaPai { get; set; }
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public string Ativo { get; set; } = "S";
}