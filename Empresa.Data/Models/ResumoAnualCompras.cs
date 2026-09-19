namespace Empresa.Data.Models;

/// <summary>
/// Resultado principal do resumo anual de compras (sp_realizado - p_resultado)
/// </summary>
public class ResumoAnualCompras
{
    public string? Estabelecimento { get; set; }
    public string? Produto { get; set; }
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

/// <summary>
/// Totais do resumo anual de compras (sp_realizado - p_total)
/// </summary>
public class ResumoAnualComprasTotais
{
    public decimal? TotalJaneiro { get; set; }
    public decimal? TotalFevereiro { get; set; }
    public decimal? TotalMarco { get; set; }
    public decimal? TotalAbril { get; set; }
    public decimal? TotalMaio { get; set; }
    public decimal? TotalJunho { get; set; }
    public decimal? TotalJulho { get; set; }
    public decimal? TotalAgosto { get; set; }
    public decimal? TotalSetembro { get; set; }
    public decimal? TotalOutubro { get; set; }
    public decimal? TotalNovembro { get; set; }
    public decimal? TotalDezembro { get; set; }
    public decimal? TotalGeral { get; set; }
}

/// <summary>
/// Dados para gráfico do resumo anual (sp_realizado - p_grafico e p_grafico2)
/// </summary>
public class ResumoAnualComprasGrafico
{
    public string? Mes { get; set; }
    public decimal? Valor { get; set; }
    public string? Tipo { get; set; }
}

/// <summary>
/// Item do comitê de compras por NF
/// </summary>
public class ComiteComprasNF
{
    public int? IdComite { get; set; }
    public string? Descricao { get; set; }
    public string? Estabelecimento { get; set; }
    public string? Produto { get; set; }
    public decimal? Quantidade { get; set; }
    public decimal? ValorUnitario { get; set; }
    public decimal? ValorTotal { get; set; }
    public DateTime? DataVigenciaInicial { get; set; }
    public DateTime? DataVigenciaFinal { get; set; }
}

/// <summary>
/// Progressão de preço por NF
/// </summary>
public class ProgressaoPrecoNF
{
    public string? Produto { get; set; }
    public string? Estabelecimento { get; set; }
    public DateTime? Data { get; set; }
    public decimal? Preco { get; set; }
    public decimal? Variacao { get; set; }
}

/// <summary>
/// Previsão de compra
/// </summary>
public class PrevisaoCompra
{
    public int IdPeriodo { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataInicial { get; set; }
    public DateTime? DataFinal { get; set; }
    public string? Produto { get; set; }
    public decimal? QuantidadePrevista { get; set; }
    public decimal? QuantidadeRealizada { get; set; }
}

/// <summary>
/// CFOP de transferência
/// </summary>
public class CfopTransferencia
{
    public int Id { get; set; }
    public string? Cfop { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataCadastro { get; set; }
    public string? UsuarioCadastro { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public string? UsuarioAlteracao { get; set; }
}

/// <summary>
/// Centro de custo
/// </summary>
public class CentroCusto
{
    public string? Conta { get; set; }
    public string? Descricao { get; set; }
    public string? CentroCustoConta { get; set; }
    public decimal? Valor { get; set; }
}