using Dapper;
using Empresa.Data.Models;
using Empresa.Data.Oracle;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Empresa.Data.Repositories;

public class AgricolaRepository : IAgricolaRepository
{
    private readonly DbSession _session;
    public AgricolaRepository(DbSession session) => _session = session;

    public async Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasAsync(DateTime data)
    {
        var proc = $"{OracleProcedures.PackageComprasBi}.{OracleProcedures.Procedures.SpRecebimentoFrutas}";

        using var cmd = (OracleCommand)_session.Connection.CreateCommand();
        cmd.CommandText = proc;
        cmd.CommandType = CommandType.StoredProcedure;
        // Fix ORA-50028: a procedure sp_recebimento_frutas espera a data no formato dd/MM/yyyy
        // (igual ao legado WebForms que passava datePesquisa.Text diretamente como string)
        cmd.Parameters.Add("p_data", OracleDbType.Varchar2).Value = data.ToString("dd/MM/yyyy");
        cmd.Parameters.Add("r_resultado", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        return await ReadDynamicResultAsync(cmd);
    }

    public async Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasDetAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada)
    {
        var proc = $"{OracleProcedures.PackageComprasBi}.{OracleProcedures.Procedures.SpRecebimentoFrutasDet}";

        using var cmd = (OracleCommand)_session.Connection.CreateCommand();
        cmd.CommandText = proc;
        cmd.CommandType = CommandType.StoredProcedure;
        // Fix ORA-50028: a procedure sp_recebimento_frutas_det espera a data no formato dd/MM/yyyy
        cmd.Parameters.Add("p_data_emissao", OracleDbType.Varchar2).Value = data.ToString("dd/MM/yyyy");
        cmd.Parameters.Add("p_empresa", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(empresa) ? DBNull.Value : (object)empresa;
        cmd.Parameters.Add("p_linha", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(linha) ? DBNull.Value : (object)linha;
        cmd.Parameters.Add("p_uf", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(uf) ? DBNull.Value : (object)uf;
        cmd.Parameters.Add("p_coluna_clicada", OracleDbType.Varchar2).Value = colunaClicada;
        cmd.Parameters.Add("r_resultado", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        return await ReadDynamicResultAsync(cmd);
    }

    public async Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasDetNfAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, string? colunaGrauClicada, string? cdVariedade)
    {
        var proc = $"{OracleProcedures.PackageComprasBi}.{OracleProcedures.Procedures.SpRecebimentoFrutasDetNf}";

        using var cmd = (OracleCommand)_session.Connection.CreateCommand();
        cmd.CommandText = proc;
        cmd.CommandType = CommandType.StoredProcedure;
        // Fix ORA-50028: a procedure sp_recebimento_frutas_det_nf espera a data no formato dd/MM/yyyy
        cmd.Parameters.Add("p_data_emissao", OracleDbType.Varchar2).Value = data.ToString("dd/MM/yyyy");
        cmd.Parameters.Add("p_empresa", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(empresa) ? DBNull.Value : (object)empresa;
        cmd.Parameters.Add("p_linha", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(linha) ? DBNull.Value : (object)linha;
        cmd.Parameters.Add("p_uf", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(uf) ? DBNull.Value : (object)uf;
        cmd.Parameters.Add("p_coluna_clicada", OracleDbType.Varchar2).Value = colunaClicada;
        cmd.Parameters.Add("p_coluna_grau_clicada", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(colunaGrauClicada) ? DBNull.Value : (object)colunaGrauClicada;
        cmd.Parameters.Add("p_cd_variedade", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(cdVariedade) ? DBNull.Value : (object)cdVariedade;
        cmd.Parameters.Add("r_resultado", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        return await ReadDynamicResultAsync(cmd);
    }

    /// <summary>
    /// Executa o OracleCommand e lê o REF CURSOR retornado dinamicamente,
    /// construindo lista de dicionários coluna→valor sem tipo fixo.
    /// </summary>
    private static async Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> ReadDynamicResultAsync(OracleCommand cmd)
    {
        // O DbSession não abre a conexão automaticamente (apenas o Dapper faz isso).
        // Precisamos abrir manualmente antes de executar o OracleCommand diretamente.
        if (cmd.Connection.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        var colunas = new List<string>();
        var linhas = new List<IDictionary<string, object>>();

        for (int i = 0; i < reader.FieldCount; i++)
            colunas.Add(reader.GetName(i));

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>(colunas.Count);
            for (int i = 0; i < colunas.Count; i++)
                row[colunas[i]] = reader.IsDBNull(i) ? "" : reader.GetValue(i);
            linhas.Add(row);
        }

        return (colunas, linhas);
    }
}


public class CalendarioRepository : ICalendarioRepository
{
    private readonly DbSession _session;
    public CalendarioRepository(DbSession session) => _session = session;

    public async Task<IEnumerable<CalendarioFinanceiro>> GetDatasAsync(int empresa, int ano, int mes)
    {
        const string sql = @"
            SELECT DATA, DIA_SEMANA AS DiaSemana, COR AS Cor, UTIL AS Util
            FROM CALENDARIO_FINANCEIRO
            WHERE EXTRACT(YEAR FROM DATA) = :Ano AND EXTRACT(MONTH FROM DATA) = :Mes
            ORDER BY DATA";
        return await _session.Connection.QueryAsync<CalendarioFinanceiro>(sql, new { Ano = ano, Mes = mes });
    }

    public async Task<bool> UpdateDiaUtilAsync(DateTime data, bool util)
    {
        const string sql = "UPDATE CALENDARIO_FINANCEIRO SET UTIL = :Util WHERE DATA = :Data";
        var rows = await _session.Connection.ExecuteAsync(sql, new { Data = data, Util = util ? "S" : "N" });
        return rows > 0;
    }
}

public class AgrupamentoRepository : IAgrupamentoRepository
{
    private readonly DbSession _session;
    public AgrupamentoRepository(DbSession session) => _session = session;

    public async Task<IEnumerable<AgrupamentoDre>> GetAllAsync()
    {
        const string sql = "SELECT ID_AGRUPAMENTO AS Id, NOME AS Nome, TIPO AS Tipo, ORDEM AS Ordem FROM AGRUPAMENTO_DRE ORDER BY ORDEM";
        return await _session.Connection.QueryAsync<AgrupamentoDre>(sql);
    }

    public async Task<int> CreateAsync(AgrupamentoDre item)
    {
        const string sql = @"INSERT INTO AGRUPAMENTO_DRE (NOME, TIPO, ORDEM) VALUES (:Nome, :Tipo, :Ordem) RETURNING ID_AGRUPAMENTO INTO :Id";
        var p = new DynamicParameters(new { item.Nome, item.Tipo, item.Ordem });
        p.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        await _session.Connection.ExecuteAsync(sql, p);
        return p.Get<int>(":Id");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM AGRUPAMENTO_DRE WHERE ID_AGRUPAMENTO = :Id";
        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}

public class DbaRepository : IDbaRepository
{
    private readonly DbSession _session;
    public DbaRepository(DbSession session) => _session = session;

    public async Task<IEnumerable<SessaoOracle>> GetSessoesAsync()
    {
        const string sql = @"
            SELECT SID AS Sid, SERIAL# AS Serial, USERNAME AS Usuario,
                   MACHINE AS Machine, STATUS AS Status, PROGRAM AS Programa,
                   LOGON_TIME AS LogonTime
            FROM V$SESSION
            WHERE TYPE != 'BACKGROUND'
            ORDER BY LOGON_TIME DESC";
        return await _session.Connection.QueryAsync<SessaoOracle>(sql);
    }

    public async Task<bool> MatarSessaoAsync(int sid, int serial)
    {
        try
        {
            await _session.Connection.ExecuteAsync(
                $"ALTER SYSTEM KILL SESSION '{sid},{serial}' IMMEDIATE");
            return true;
        }
        catch { return false; }
    }

    public async Task<IEnumerable<TabelaLock>> GetLocksAsync()
    {
        const string sql = @"
            SELECT l.SESSION_ID AS Sid, o.OBJECT_NAME AS Tabela,
                   l.LOCKED_MODE AS TipoLock, s.USERNAME AS Usuario
            FROM V$LOCKED_OBJECT l
            JOIN DBA_OBJECTS o ON l.OBJECT_ID = o.OBJECT_ID
            JOIN V$SESSION s ON l.SESSION_ID = s.SID";
        return await _session.Connection.QueryAsync<TabelaLock>(sql);
    }
}
