using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de páginas — CRUD das páginas do sistema
/// </summary>
public class PaginaRepository : IPaginaRepository
{
    private readonly DbSession _session;

    public PaginaRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Pagina>> GetAllAsync()
    {
        const string sql = @"
            SELECT ID_PAGINA, URL, TITULO_ABA, CHAVE_CONTROLE,
                   TITULO_MENU, ID_PAGINA_PAI, ORDEM, TOOLTIP, ATIVO
            FROM ACESSO_CADASTRO_PAGINA
            ORDER BY ORDEM";

        return await _session.Connection.QueryAsync<Pagina>(sql);
    }

    public async Task<Pagina?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT ID_PAGINA, URL, TITULO_ABA, CHAVE_CONTROLE,
                   TITULO_MENU, ID_PAGINA_PAI, ORDEM, TOOLTIP, ATIVO
            FROM ACESSO_CADASTRO_PAGINA
            WHERE ID_PAGINA = :Id";

        return await _session.Connection.QueryFirstOrDefaultAsync<Pagina>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Pagina pagina)
    {
        const string sql = @"
            INSERT INTO ACESSO_CADASTRO_PAGINA (URL, TITULO_ABA, CHAVE_CONTROLE, TITULO_MENU,
                                ID_PAGINA_PAI, ORDEM, TOOLTIP, ATIVO)
            VALUES (:Url, :TituloAba, :ChaveControle, :TituloMenu,
                    :IDPaginaPai, :Ordem, :ToolTip, :Ativo)
            RETURNING ID_PAGINA INTO :Id";

        var parameters = new DynamicParameters(new
        {
            pagina.Url,
            pagina.TituloAba,
            pagina.ChaveControle,
            pagina.TituloMenu,
            pagina.IDPaginaPai,
            pagina.Ordem,
            pagina.ToolTip,
            pagina.Ativo
        });
        parameters.Add(":Id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> UpdateAsync(Pagina pagina)
    {
        const string sql = @"
            UPDATE ACESSO_CADASTRO_PAGINA
            SET URL = :Url,
                TITULO_ABA = :TituloAba,
                CHAVE_CONTROLE = :ChaveControle,
                TITULO_MENU = :TituloMenu,
                ID_PAGINA_PAI = :IDPaginaPai,
                ORDEM = :Ordem,
                TOOLTIP = :ToolTip,
                ATIVO = :Ativo
            WHERE ID_PAGINA = :IDPagina";

        var rows = await _session.Connection.ExecuteAsync(sql, pagina);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE ACESSO_CADASTRO_PAGINA
            SET ATIVO = 'N'
            WHERE ID_PAGINA = :Id";

        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}