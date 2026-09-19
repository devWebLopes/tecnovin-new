namespace Empresa.Data.Models;

// ─── Nível 1: Mensal ──────────────────────────
public class PrazoMedioMensal
{
    public string? Mes { get; set; }
    public int? Dias { get; set; }
    public decimal? Valor { get; set; }
    public decimal? PrazoMedio { get; set; }
}

// ─── Nível 2: Pessoa ──────────────────────────
public class PrazoMedioPessoa
{
    public string? Pessoa { get; set; }
    public decimal? Valor { get; set; }
    public decimal? PrazoMedio { get; set; }
    public int? QuantidadeDocumentos { get; set; }
}

// ─── Nível 3: Documentos ───────────────────────
public class PrazoMedioDocumento
{
    public string? Documento { get; set; }
    public string? Pessoa { get; set; }
    public DateTime? Emissao { get; set; }
    public DateTime? Vencimento { get; set; }
    public decimal? Valor { get; set; }
    public int? Dias { get; set; }
    public string? TipoOperacao { get; set; }
}