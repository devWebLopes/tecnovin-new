using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de prazo médio — 3 níveis de drill-down (mensal, pessoa, documento)
/// </summary>
public interface IPrazoMedioRepository
{
    // ─── RECEBIMENTO ─────────────────────────────────────
    Task<IEnumerable<PrazoMedioMensal>> GetRecebimentoMensalAsync(int empresa, int ano);
    Task<IEnumerable<PrazoMedioPessoa>> GetRecebimentoPessoaAsync(int empresa, int ano, string mes);
    Task<IEnumerable<PrazoMedioDocumento>> GetRecebimentoDocumentosAsync(int empresa, string pessoa, int ano, string mes);

    // ─── PAGAMENTO ───────────────────────────────────────
    Task<IEnumerable<PrazoMedioMensal>> GetPagamentoMensalAsync(int empresa, int ano, string? tipoOperacao);
    Task<IEnumerable<PrazoMedioPessoa>> GetPagamentoPessoaAsync(int empresa, int ano, string mes);
    Task<IEnumerable<PrazoMedioDocumento>> GetPagamentoDocumentosAsync(int empresa, string pessoa, int ano, string mes);
}