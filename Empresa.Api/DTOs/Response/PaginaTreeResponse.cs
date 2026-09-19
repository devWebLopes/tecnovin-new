namespace Empresa.Api.DTOs.Response;

/// <summary>
/// DTO hierárquico para árvore de páginas com indicador de vínculo ao perfil
/// </summary>
public class PaginaTreeResponse
{
    public int IdPagina { get; set; }
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IdPaginaPai { get; set; }
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Vinculado { get; set; }
    public List<PaginaTreeResponse> Filhos { get; set; } = new();
}