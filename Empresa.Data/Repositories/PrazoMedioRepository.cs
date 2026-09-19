using Dapper;
using Empresa.Data.Models;
using System.Data;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de prazo médio — 3 níveis de drill-down usando funções table-valued Oracle
/// </summary>
public class PrazoMedioRepository : IPrazoMedioRepository
{
    private readonly DbSession _session;

    public PrazoMedioRepository(DbSession session)
    {
        _session = session;
    }

    // ─── RECEBIMENTO ─────────────────────────────────────

    public async Task<IEnumerable<PrazoMedioMensal>> GetRecebimentoMensalAsync(int empresa, int ano)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_RECEBIMENTO_MENSAL(:Empresa, :Ano))";

        return await _session.Connection.QueryAsync<PrazoMedioMensal>(sql, new { Empresa = empresa, Ano = ano });
    }

    public async Task<IEnumerable<PrazoMedioPessoa>> GetRecebimentoPessoaAsync(int empresa, int ano, string mes)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_RECEBIMENTO_PESSOA(:Empresa, :Ano, :Mes))";

        return await _session.Connection.QueryAsync<PrazoMedioPessoa>(sql, new { Empresa = empresa, Ano = ano, Mes = mes });
    }

    public async Task<IEnumerable<PrazoMedioDocumento>> GetRecebimentoDocumentosAsync(int empresa, string pessoa, int ano, string mes)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_RECEBIMENTO(:Empresa, :Ano, :Mes, :Pessoa))";

        return await _session.Connection.QueryAsync<PrazoMedioDocumento>(sql,
            new { Empresa = empresa, Ano = ano, Mes = mes, Pessoa = pessoa });
    }

    // ─── PAGAMENTO ───────────────────────────────────────

    public async Task<IEnumerable<PrazoMedioMensal>> GetPagamentoMensalAsync(int empresa, int ano, string? tipoOperacao)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_PAGAMENTO_MENSAL(:Empresa, :Ano))";

        var result = await _session.Connection.QueryAsync<PrazoMedioMensal>(sql,
            new { Empresa = empresa, Ano = ano });

        // Filtro por tipo de operação (G, I, O, U, L, M, S)
        if (!string.IsNullOrEmpty(tipoOperacao))
            result = result.Where(r => r.Mes?.StartsWith(tipoOperacao) == true);

        return result;
    }

    public async Task<IEnumerable<PrazoMedioPessoa>> GetPagamentoPessoaAsync(int empresa, int ano, string mes)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_PAGAMENTO_PESSOA(:Empresa, :Ano, :Mes))";

        return await _session.Connection.QueryAsync<PrazoMedioPessoa>(sql,
            new { Empresa = empresa, Ano = ano, Mes = mes });
    }

    public async Task<IEnumerable<PrazoMedioDocumento>> GetPagamentoDocumentosAsync(int empresa, string pessoa, int ano, string mes)
    {
        const string sql = @"
            SELECT * FROM TABLE(
                PKG_PRAZO_MEDIO.FN_PAGAMENTO(:Empresa, :Ano, :Mes, :Pessoa))";

        return await _session.Connection.QueryAsync<PrazoMedioDocumento>(sql,
            new { Empresa = empresa, Ano = ano, Mes = mes, Pessoa = pessoa });
    }
}