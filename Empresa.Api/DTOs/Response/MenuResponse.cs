namespace Empresa.Api.DTOs.Response;

/// <summary>
/// Shell completo do menu (GET /api/v1/menu): árvore hierárquica, mais acessados e usuário/B.I.
/// </summary>
public class MenuResponse
{
    public List<MenuItemResponse> Menu { get; set; } = new();
    public List<MaisAcessadoItemResponse> MaisAcessados { get; set; } = new();
    public MenuUsuarioResponse Usuario { get; set; } = new();
}
