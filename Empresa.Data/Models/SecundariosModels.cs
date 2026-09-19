namespace Empresa.Data.Models;

// ─── Calendário Financeiro ───────────────────
public class CalendarioFinanceiro
{
    public DateTime Data { get; set; }
    public string? DiaSemana { get; set; }
    public string? Cor { get; set; } // vermelho, azul, amarelo
    public bool Util { get; set; }
}

// ─── Agrupamento DRE ─────────────────────────
public class AgrupamentoDre
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Tipo { get; set; }
    public int Ordem { get; set; }
}

// ─── DBA ─────────────────────────────────────
public class SessaoOracle
{
    public int Sid { get; set; }
    public int Serial { get; set; }
    public string? Usuario { get; set; }
    public string? Machine { get; set; }
    public string? Status { get; set; }
    public string? Programa { get; set; }
    public DateTime? LogonTime { get; set; }
}

public class TabelaLock
{
    public int Sid { get; set; }
    public string? Tabela { get; set; }
    public string? TipoLock { get; set; }
    public string? Usuario { get; set; }
}