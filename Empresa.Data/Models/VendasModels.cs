namespace Empresa.Data.Models;

/// <summary>
/// Análise de vendas — mercado interno
/// </summary>
public class AnaliseVendas
{
    public string? Produto { get; set; }
    public string? Cliente { get; set; }
    public decimal? Quantidade { get; set; }
    public decimal? Valor { get; set; }
    public decimal? Percentual { get; set; }
}

/// <summary>
/// Ranking de clientes por receita
/// </summary>
public class RankingCliente
{
    public int? Posicao { get; set; }
    public string? Cliente { get; set; }
    public decimal? Receita { get; set; }
    public decimal? Percentual { get; set; }
    public decimal? Acumulado { get; set; }
}

/// <summary>
/// Plano de vendas vs resultado
/// </summary>
public class PlanoVendasResultado
{
    public string? Produto { get; set; }
    public decimal? Meta { get; set; }
    public decimal? Realizado { get; set; }
    public decimal? Atingimento { get; set; }
}

/// <summary>
/// Comercial Mercado Interno — com pós-processamento
/// </summary>
public class ComercialMI
{
    public string? Periodo { get; set; }
    public string? Produto { get; set; }
    public decimal? Valor { get; set; }
    public decimal? Diferenca { get; set; }
}