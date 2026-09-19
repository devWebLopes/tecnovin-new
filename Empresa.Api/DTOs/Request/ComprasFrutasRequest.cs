namespace Empresa.Api.DTOs.Request;

public class ComprasFrutasRequest
{
    public DateTime? Data { get; set; }
    public string? Empresa { get; set; }
    public string? Linha { get; set; }
    public string? Uf { get; set; }
    public string? ColunaClicada { get; set; }
    public string? ColunaGrauClicada { get; set; }
    public string? Variedade { get; set; }
}
