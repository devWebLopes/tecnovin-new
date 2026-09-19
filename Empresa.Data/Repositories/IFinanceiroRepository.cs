using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

public interface IFinanceiroRepository
{
    // ─── Posição Financeira (3 versões) ────────────────
    Task<IEnumerable<PosicaoFinanceira>> GetPosicaoFinanceiraAsync(int empresa, string versao, DateTime? dataInicial, DateTime? dataFinal);
    Task<IEnumerable<PosicaoFinanceiraResumo>> GetResumoPosicaoAsync(int empresa);
    Task<IEnumerable<PosicaoSemanal>> GetPosicaoSemanalAsync(int empresa);

    // ─── Fluxo de Caixa ────────────────────────────────
    Task<IEnumerable<FluxoCaixaMaster>> GetFluxoCaixaAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal);
    Task<IEnumerable<FluxoCaixaDetalhe>> GetFluxoCaixaAnaliticoAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal);
    Task<FluxoCaixaTotais?> GetTotaisFluxoCaixaAsync(int empresa);

    // ─── DRE ────────────────────────────────────────────
    Task<IEnumerable<Dre>> GetDreAsync(int empresa, int ano, string versao);
    Task<IEnumerable<DreDocumento>> GetDreDocumentosAsync(int empresa, string conta, string versao);

    // ─── Projeção ───────────────────────────────────────
    Task<IEnumerable<ProjecaoFinanceira>> GetProjecaoAsync(int empresa, int ano);
    Task<IEnumerable<PercentualAgrupamento>> GetPercentuaisProjecaoAsync(int empresa);

    // ─── Ajustes ────────────────────────────────────────
    Task<IEnumerable<AjusteFinanceiro>> GetAjustesAsync(int empresa);
    Task<int> CreateAjusteAsync(AjusteFinanceiro ajuste);
    Task<bool> InativarAjusteAsync(int id);

    // ─── Portador ────────────────────────────────────────
    Task<IEnumerable<Portador>> GetPortadoresAsync();
    Task<bool> SetSaldoInicialAsync(int idPortador, decimal saldo);
}