using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de Acesso B.I. — gerencia a tabela USUARIO_EMPRESA via Dapper.
/// Queries semanticamente idênticas ao legado (Q2, Q3, Q6, Q7, Q8, Q9) com parâmetros nomeados.
/// </summary>
public class AcessoBiRepository : IAcessoBiRepository
{
    private readonly DbSession _session;

    public AcessoBiRepository(DbSession session)
    {
        _session = session;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioBi>> GetUsuariosComAcessoAsync(string? search = null)
    {
        const string sql = @"
            SELECT U.ID_USUARIO,
                   A.NOME,
                   LISTAGG(SUBSTR(V.NM_FANTASIA, 1, INSTR(V.NM_FANTASIA, ' ') - 1), ' - ')
                       WITHIN GROUP (ORDER BY U.EMPRESA) AS EMPRESAS
              FROM USUARIO_EMPRESA U
             INNER JOIN ACESSO_CADASTRO_USUARIO A ON U.ID_USUARIO = A.ID_USUARIO
             INNER JOIN VW_EMPRESA_NEW V          ON U.EMPRESA    = V.CD_EMPRESA
             WHERE (:Search IS NULL OR A.NOME LIKE '%' || :Search || '%')
             GROUP BY U.ID_USUARIO, A.NOME
             ORDER BY A.NOME";

        return await _session.Connection.QueryAsync<UsuarioBi>(
            sql, new { Search = search });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioEmpresa>> GetEmpresasDoUsuarioAsync(long idUsuario)
    {
        const string sql = @"
            SELECT B.ID_USUARIO_EMPRESA,
                   B.ID_USUARIO,
                   B.EMPRESA,
                   E.NM_FANTASIA AS NOME_FANTASIA
              FROM USUARIO_EMPRESA B
             INNER JOIN VW_EMPRESA_NEW E ON B.EMPRESA = E.CD_EMPRESA
             WHERE B.ID_USUARIO = :IdUsuario
             ORDER BY E.NM_FANTASIA";

        return await _session.Connection.QueryAsync<UsuarioEmpresa>(
            sql, new { IdUsuario = idUsuario });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmpresaDisponivel>> GetEmpresasDisponiveisAsync(long idUsuario)
    {
        const string sql = @"
            SELECT CD_EMPRESA, NM_FANTASIA
              FROM VW_EMPRESA_NEW
             WHERE CD_EMPRESA NOT IN (SELECT NVL(EMPRESA, 0)
                                        FROM USUARIO_EMPRESA
                                       WHERE ID_USUARIO = :IdUsuario)
             ORDER BY NM_FANTASIA";

        return await _session.Connection.QueryAsync<EmpresaDisponivel>(
            sql, new { IdUsuario = idUsuario });
    }

    /// <inheritdoc />
    public async Task ConcederAcessoTotalAsync(long idUsuario)
    {
        const string sql = @"
            INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
            SELECT NULL, :IdUsuario, CD_EMPRESA
              FROM VW_EMPRESA_NEW";

        await _session.Connection.ExecuteAsync(sql, new { IdUsuario = idUsuario });
    }

    /// <inheritdoc />
    public async Task ConcederAcessoEmpresaAsync(long idUsuario, long codigoEmpresa)
    {
        const string sql = @"
            INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
            VALUES (NULL, :IdUsuario, :Empresa)";

        await _session.Connection.ExecuteAsync(sql, new { IdUsuario = idUsuario, Empresa = codigoEmpresa });
    }

    /// <inheritdoc />
    public async Task<bool> RemoverAcessoAsync(long idUsuarioEmpresa)
    {
        const string sql = @"
            DELETE FROM USUARIO_EMPRESA WHERE ID_USUARIO_EMPRESA = :IdUsuarioEmpresa";

        var affected = await _session.Connection.ExecuteAsync(sql, new { IdUsuarioEmpresa = idUsuarioEmpresa });
        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UsuarioPossuiAcessoAsync(long idUsuario)
    {
        const string sql = @"
            SELECT COUNT(1)
              FROM USUARIO_EMPRESA
             WHERE ID_USUARIO = :IdUsuario";

        var count = await _session.Connection.ExecuteScalarAsync<long>(sql, new { IdUsuario = idUsuario });
        return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> VinculoJaExisteAsync(long idUsuario, long codigoEmpresa)
    {
        const string sql = @"
            SELECT COUNT(1)
              FROM USUARIO_EMPRESA
             WHERE ID_USUARIO = :IdUsuario
               AND EMPRESA = :Empresa";

        var count = await _session.Connection.ExecuteScalarAsync<long>(
            sql, new { IdUsuario = idUsuario, Empresa = codigoEmpresa });
        return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UsuarioExisteAsync(long idUsuario)
    {
        const string sql = @"
            SELECT COUNT(1)
              FROM ACESSO_CADASTRO_USUARIO
             WHERE ID_USUARIO = :IdUsuario";

        var count = await _session.Connection.ExecuteScalarAsync<long>(sql, new { IdUsuario = idUsuario });
        return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> EmpresaExisteAsync(long codigoEmpresa)
    {
        const string sql = @"
            SELECT COUNT(1)
              FROM VW_EMPRESA_NEW
             WHERE CD_EMPRESA = :CodigoEmpresa";

        var count = await _session.Connection.ExecuteScalarAsync<long>(sql, new { CodigoEmpresa = codigoEmpresa });
        return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> VinculoExisteAsync(long idUsuarioEmpresa)
    {
        const string sql = @"
            SELECT COUNT(1)
              FROM USUARIO_EMPRESA
             WHERE ID_USUARIO_EMPRESA = :IdUsuarioEmpresa";

        var count = await _session.Connection.ExecuteScalarAsync<long>(sql, new { IdUsuarioEmpresa = idUsuarioEmpresa });
        return count > 0;
    }
}