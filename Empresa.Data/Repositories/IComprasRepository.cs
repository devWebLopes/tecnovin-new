using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de compras — mapeia DaoCompras do legado
/// </summary>
public interface IComprasRepository
{
    // ─── Resumo Anual / Comitê ───────────────────────────
    Task<IEnumerable<ResumoAnualCompras>> GetResumoAnualAsync(int empresa, int? estabelecimento);
    Task<ResumoAnualComprasTotais?> GetResumoAnualTotaisAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<ResumoAnualComprasGrafico>> GetResumoAnualGraficoAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<ComiteComprasNF>> GetComiteComprasNFAsync(int empresa, DateTime dataInicial, DateTime dataFinal);

    // ─── Previsto vs Realizado ───────────────────────────
    Task<IEnumerable<PrevisaoCompra>> GetPrevistoRealizadoAsync(int empresa, int? estabelecimento);

    // ─── Progressão de Preço ─────────────────────────────
    Task<IEnumerable<ProgressaoPrecoNF>> GetProgressaoPrecoAsync(int empresa, string? produto);

    // ─── CFOP ────────────────────────────────────────────
    Task<IEnumerable<CfopTransferencia>> GetCfopTransferenciasAsync();
    Task<int> CreateCfopTransferenciaAsync(CfopTransferencia cfop);
    Task<bool> DeleteCfopTransferenciaAsync(int id);

    // ─── Centro de Custo ─────────────────────────────────
    Task<IEnumerable<CentroCusto>> GetCentroCustoAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheAsync(string conta, int? estabelecimento);
    Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheProdutoAsync(string conta, int? estabelecimento);
}