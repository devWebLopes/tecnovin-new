namespace Empresa.Data.Models;

/// <summary>
/// Entidade que representa a tabela META_COMPRAS — metas de volume de compra de frutas por safra, linha de produto e empresa.
/// Fonte: docs/agricola_regra.md (Painel 1 — Cad. Safra/Meta)
/// </summary>
public class MetaCompra
{
    public long IdMetaCompra { get; set; }
    public long CdLinha { get; set; }
    public string Safra { get; set; } = string.Empty;
    public DateTime DtInicial { get; set; }
    public DateTime DtFinal { get; set; }
    public decimal MetaQtde { get; set; }
    public long CdEmpresa { get; set; }
}
