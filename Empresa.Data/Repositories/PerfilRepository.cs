using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de perfis — CRUD com CONNECT BY PRIOR para árvore e MERGE para permissões
/// </summary>
public class PerfilRepository : IPerfilRepository
{
    private readonly DbSession _session;

    public PerfilRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Perfil>> GetAllAsync(string? search = null)
    {
        var sql = @"
            SELECT ID_PERFIL, DESCRICAO
            FROM ACESSO_CADASTRO_PERFIL
            WHERE (:search IS NULL OR UPPER(DESCRICAO) LIKE UPPER(:search || '%'))
            ORDER BY DESCRICAO";

        return await _session.Connection.QueryAsync<Perfil>(sql, new { search });
    }

    public async Task<Perfil?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT ID_PERFIL, DESCRICAO
            FROM ACESSO_CADASTRO_PERFIL
            WHERE ID_PERFIL = :Id";

        return await _session.Connection.QueryFirstOrDefaultAsync<Perfil>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Perfil perfil)
    {
        const string sql = @"
            INSERT INTO ACESSO_CADASTRO_PERFIL (DESCRICAO)
            VALUES (:Descricao)
            RETURNING ID_PERFIL INTO :Id";

        var parameters = new DynamicParameters(new { perfil.Descricao });
        parameters.Add(":Id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> UpdateAsync(Perfil perfil)
    {
        const string sql = @"
            UPDATE ACESSO_CADASTRO_PERFIL
            SET DESCRICAO = :Descricao
            WHERE ID_PERFIL = :IDPerfil";

        var rows = await _session.Connection.ExecuteAsync(sql, perfil);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            DELETE FROM ACESSO_CADASTRO_PERFIL
            WHERE ID_PERFIL = :Id";

        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<IEnumerable<PaginaTree>> GetPaginasTreeAsync(int idPerfil)
    {
        const string sql = @"
            SELECT p.ID_PAGINA,
                   p.URL,
                   p.TITULO_ABA,
                   p.CHAVE_CONTROLE,
                   p.TITULO_MENU,
                   p.ID_PAGINA_PAI,
                   p.ORDEM,
                   p.TOOLTIP,
                   p.ATIVO,
                   CASE WHEN EXISTS (
                       SELECT 1 
                       FROM ACESSO_PERFIL_PAGINA pp 
                       WHERE pp.ID_PAGINA = p.ID_PAGINA 
                         AND pp.ID_PERFIL = :IdPerfil
                   ) THEN 'S' ELSE 'N' END AS VINCULADO
            FROM ACESSO_CADASTRO_PAGINA p
            WHERE p.ATIVO = 'S'
            ORDER BY NVL(p.ID_PAGINA_PAI, 0), p.ORDEM";

        return await _session.Connection.QueryAsync<PaginaTree>(sql, new { IdPerfil = idPerfil });
    }

    public async Task SalvarPermissoesAsync(int idPerfil, List<int> vincularIds, List<int> desvincularIds)
    {
        using var transaction = _session.Connection.BeginTransaction();

        try
        {
            // Processar vínculos a adicionar com MERGE (idempotente)
            if (vincularIds.Count > 0)
            {
                // Converter para DataTable para bulk bind em Oracle
                var sqlVincular = @"
                    MERGE INTO ACESSO_PERFIL_PAGINA t
                    USING (SELECT :IdPerfil AS ID_PERFIL, :IdPagina AS ID_PAGINA FROM DUAL) s
                    ON (t.ID_PERFIL = s.ID_PERFIL AND t.ID_PAGINA = s.ID_PAGINA)
                    WHEN NOT MATCHED THEN
                        INSERT (ID_PERFIL, ID_PAGINA)
                        VALUES (s.ID_PERFIL, s.ID_PAGINA)";

                foreach (var idPagina in vincularIds)
                {
                    await _session.Connection.ExecuteAsync(
                        sqlVincular,
                        new { IdPerfil = idPerfil, IdPagina = idPagina },
                        transaction);
                }
            }

            // Processar vínculos a remover
            if (desvincularIds.Count > 0)
            {
                const string sqlDesvincular = @"
                    DELETE FROM ACESSO_PERFIL_PAGINA
                    WHERE ID_PERFIL = :IdPerfil
                      AND ID_PAGINA = :IdPagina";

                foreach (var idPagina in desvincularIds)
                {
                    await _session.Connection.ExecuteAsync(
                        sqlDesvincular,
                        new { IdPerfil = idPerfil, IdPagina = idPagina },
                        transaction);
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}