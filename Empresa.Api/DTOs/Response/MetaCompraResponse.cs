namespace Empresa.Api.DTOs.Response;

public class MetaCompraResponse
{
    public long IdMetaCompra { get; set; }
    public long CdLinha { get; set; }
    public string Safra { get; set; } = string.Empty;
    public string DtInicial { get; set; } = string.Empty;
    public string DtFinal { get; set; } = string.Empty;
    public decimal MetaQtde { get; set; }
    public long CdEmpresa { get; set; }
}
