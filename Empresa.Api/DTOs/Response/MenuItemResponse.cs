namespace Empresa.Api.DTOs.Response;

/// <summary>
/// Nó da árvore de menu — recursivo (pais → filhos ordenados por Ordem, RN-05).
/// Substitui o transportador abusado `url|tituloAba` do legado (D-10).
/// </summary>
public class MenuItemResponse
{
    public int IdPagina { get; set; }
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ToolTip { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public List<MenuItemResponse> Filhos { get; set; } = new();
}
