namespace Empresa.Api.DTOs.Response;

/// <summary>
/// DTO hierárquico para árvore de empresas/estabelecimentos com indicador de vínculo ao usuário
/// </summary>
public class EstabelecimentoTreeResponse
{
    public int CdEmpresa { get; set; }
    public string DsEmpresa { get; set; } = string.Empty;
    public List<EstabelecimentoFilhoResponse> Estabelecimentos { get; set; } = new();
}

/// <summary>
/// DTO para nó filho (estabelecimento) na árvore
/// </summary>
public class EstabelecimentoFilhoResponse
{
    public int CdEstabelecimento { get; set; }
    public string DsEstabelecimento { get; set; } = string.Empty;
    public bool Vinculado { get; set; }
}