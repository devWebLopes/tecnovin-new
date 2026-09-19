namespace Empresa.Data.Models;

// ─── Posição Financeira ───────────────────────
public class PosicaoFinanceira
{
    public string? Agrupamento { get; set; }
    public string? Conta { get; set; }
    public decimal? Valor { get; set; }
    public decimal? Previsto { get; set; }
    public decimal? Realizado { get; set; }
    public decimal? Saldo { get; set; }
}

public class PosicaoFinanceiraResumo
{
    public decimal? TotalEntradas { get; set; }
    public decimal? TotalSaidas { get; set; }
    public decimal? SaldoFinal { get; set; }
    public decimal? Externo { get; set; }
    public decimal? Aplicacoes { get; set; }
}

public class PosicaoSemanal
{
    public string? Semana { get; set; }
    public string? Conta { get; set; }
    public decimal? Segunda { get; set; }
    public decimal? Terca { get; set; }
    public decimal? Quarta { get; set; }
    public decimal? Quinta { get; set; }
    public decimal? Sexta { get; set; }
    public decimal? Sabado { get; set; }
    public decimal? Domingo { get; set; }
    public decimal? Total { get; set; }
}

// ─── Fluxo de Caixa ───────────────────────────
public class FluxoCaixaMaster
{
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public DateTime? Data { get; set; }
    public string? Portador { get; set; }
}

public class FluxoCaixaDetalhe
{
    public string? Documento { get; set; }
    public string? ClienteFornecedor { get; set; }
    public decimal? Valor { get; set; }
    public DateTime? Vencimento { get; set; }
    public string? Portador { get; set; }
}

public class FluxoCaixaTotais
{
    public decimal? TotalEntradas { get; set; }
    public decimal? TotalSaidas { get; set; }
    public decimal? Saldo { get; set; }
}

// ─── DRE ──────────────────────────────────────
public class Dre
{
    public string? Conta { get; set; }
    public string? Descricao { get; set; }
    public decimal? Janeiro { get; set; }
    public decimal? Fevereiro { get; set; }
    public decimal? Marco { get; set; }
    public decimal? Abril { get; set; }
    public decimal? Maio { get; set; }
    public decimal? Junho { get; set; }
    public decimal? Julho { get; set; }
    public decimal? Agosto { get; set; }
    public decimal? Setembro { get; set; }
    public decimal? Outubro { get; set; }
    public decimal? Novembro { get; set; }
    public decimal? Dezembro { get; set; }
    public decimal? Total { get; set; }
}

public class DreDocumento
{
    public string? Documento { get; set; }
    public string? ClienteFornecedor { get; set; }
    public decimal? Valor { get; set; }
    public DateTime? Data { get; set; }
    public string? Historico { get; set; }
}

// ─── Projeção Financeira ──────────────────────
public class ProjecaoFinanceira
{
    public string? Conta { get; set; }
    public decimal? Projetado { get; set; }
    public decimal? Realizado { get; set; }
    public decimal? Diferenca { get; set; }
}

public class PercentualAgrupamento
{
    public string? Agrupamento { get; set; }
    public decimal? Percentual { get; set; }
}

// ─── Ajuste Financeiro ────────────────────────
public class AjusteFinanceiro
{
    public int Id { get; set; }
    public string? Documento { get; set; }
    public decimal? ValorOriginal { get; set; }
    public decimal? ValorAjustado { get; set; }
    public string? Status { get; set; }
    public string? Usuario { get; set; }
    public DateTime? DataAjuste { get; set; }
}

// ─── Portador ─────────────────────────────────
public class Portador
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public decimal? Saldo { get; set; }
    public DateTime? DataSaldo { get; set; }
}