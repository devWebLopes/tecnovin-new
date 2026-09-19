namespace Empresa.Api.DTOs.Response;

public class GridDinamicaResponse
{
    public string DataReferencia { get; set; } = string.Empty;
    public string UltimaAtualizacao { get; set; } = string.Empty;
    public IReadOnlyList<string> Colunas { get; set; } = Array.Empty<string>();
    public IEnumerable<IDictionary<string, object>> Linhas { get; set; } = Array.Empty<IDictionary<string, object>>();
}
