using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de usuários — CRUD completo com exclusão lógica e vínculos com estabelecimentos
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbSession _session;

    public UsuarioRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync(string? search = null, string? ativo = null)
    {
        var sql = @"
            SELECT u.ID_USUARIO, u.NOME, u.LOGIN, u.ID_PERFIL,
                   u.QUANTIDADE_ACESSO, u.ATUALIZA_SENHA, u.ATIVO,
                   p.DESCRICAO AS DescricaoPerfil
            FROM ACESSO_CADASTRO_USUARIO u
            LEFT JOIN ACESSO_CADASTRO_PERFIL p ON p.ID_PERFIL = u.ID_PERFIL
            WHERE (:search IS NULL OR UPPER(u.NOME) LIKE UPPER(:search || '%'))
              AND (:ativo IS NULL OR u.ATIVO = :ativo)
            ORDER BY u.NOME";

        return await _session.Connection.QueryAsync<Usuario>(sql, new { search, ativo });
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT u.ID_USUARIO, u.NOME, u.LOGIN, u.ID_PERFIL,
                   u.QUANTIDADE_ACESSO, u.ATUALIZA_SENHA, u.ATIVO,
                   p.DESCRICAO AS DescricaoPerfil
            FROM ACESSO_CADASTRO_USUARIO u
            LEFT JOIN ACESSO_CADASTRO_PERFIL p ON p.ID_PERFIL = u.ID_PERFIL
            WHERE u.ID_USUARIO = :Id";

        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<Usuario?> GetByLoginAsync(string login)
    {
        const string sql = @"
            SELECT u.ID_USUARIO, u.NOME, u.LOGIN, u.ID_PERFIL,
                   u.QUANTIDADE_ACESSO, u.ATUALIZA_SENHA, u.SENHA, u.ATIVO,
                   p.DESCRICAO AS DescricaoPerfil
            FROM ACESSO_CADASTRO_USUARIO u
            LEFT JOIN ACESSO_CADASTRO_PERFIL p ON p.ID_PERFIL = u.ID_PERFIL
            WHERE u.LOGIN = :Login AND u.ATIVO = 'S'";

        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Login = login });
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        const string sql = @"
            INSERT INTO ACESSO_CADASTRO_USUARIO (NOME, LOGIN, SENHA, ID_PERFIL, ATIVO, ATUALIZA_SENHA, QUANTIDADE_ACESSO)
            VALUES (:Nome, :Login, :Senha, :IDPerfil, :Ativo, :AtualizaSenha, 0)
            RETURNING ID_USUARIO INTO :Id";

        var parameters = new DynamicParameters(new
        {
            usuario.Nome,
            usuario.Login,
            usuario.Senha,
            usuario.IDPerfil,
            usuario.Ativo,
            usuario.AtualizaSenha
        });
        parameters.Add(":Id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        var sql = @"
            UPDATE ACESSO_CADASTRO_USUARIO
            SET NOME = :Nome,
                LOGIN = :Login,
                ID_PERFIL = :IDPerfil,
                ATIVO = :Ativo,
                ATUALIZA_SENHA = :AtualizaSenha";

        // Só atualiza senha se foi fornecida
        if (!string.IsNullOrWhiteSpace(usuario.Senha))
        {
            sql += ", SENHA = :Senha";
        }

        sql += " WHERE ID_USUARIO = :IDUsuario";

        var rows = await _session.Connection.ExecuteAsync(sql, usuario);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE ACESSO_CADASTRO_USUARIO
            SET ATIVO = 'N'
            WHERE ID_USUARIO = :Id";

        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> LoginExistsAsync(string login)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM ACESSO_CADASTRO_USUARIO
            WHERE LOGIN = :Login";

        var count = await _session.Connection.ExecuteScalarAsync<int>(sql, new { Login = login });
        return count > 0;
    }

    public async Task<IEnumerable<UsuarioEstabelecimentoTree>> GetEstabelecimentosTreeAsync(int idUsuario)
    {
        const string sql = @"
            SELECT v.CD_EMPRESA AS CdEmpresa,
                   v.ESTABELECIMENTO AS CdEstabelecimento,
                   v.DESCRITIVO,
                   CASE WHEN ue.ID_USUARIO IS NOT NULL THEN 'S' ELSE 'N' END AS VINCULADO
            FROM VW_ESTABELECIMENTO_NEW v
            LEFT JOIN ACESSO_USUARIO_EMPRESA_ESTAB ue
                ON ue.ID_USUARIO = :IdUsuario
               AND ue.CD_EMPRESA = v.CD_EMPRESA
               AND ue.CD_ESTABELECIMENTO = v.ESTABELECIMENTO
            ORDER BY v.CD_EMPRESA, v.ESTABELECIMENTO";

        return await _session.Connection.QueryAsync<UsuarioEstabelecimentoTree>(sql, new { IdUsuario = idUsuario });
    }

    public async Task SincronizarEstabelecimentosAsync(int idUsuario, List<VinculoEstabelecimento> vincular, List<VinculoEstabelecimento> desvincular)
    {
        using var transaction = _session.Connection.BeginTransaction();

        try
        {
            // Processar vínculos a adicionar (verificar duplicidade)
            foreach (var vinculo in vincular)
            {
                const string sqlCheck = @"
                    SELECT COUNT(1)
                    FROM ACESSO_USUARIO_EMPRESA_ESTAB
                    WHERE ID_USUARIO = :IdUsuario
                      AND CD_EMPRESA = :CdEmpresa
                      AND CD_ESTABELECIMENTO = :CdEstabelecimento";

                var exists = await _session.Connection.ExecuteScalarAsync<int>(
                    sqlCheck,
                    new { IdUsuario = idUsuario, vinculo.CdEmpresa, vinculo.CdEstabelecimento },
                    transaction);

                if (exists == 0)
                {
                    const string sqlInsert = @"
                        INSERT INTO ACESSO_USUARIO_EMPRESA_ESTAB (ID_USUARIO, CD_EMPRESA, CD_ESTABELECIMENTO)
                        VALUES (:IdUsuario, :CdEmpresa, :CdEstabelecimento)";

                    await _session.Connection.ExecuteAsync(
                        sqlInsert,
                        new { IdUsuario = idUsuario, vinculo.CdEmpresa, vinculo.CdEstabelecimento },
                        transaction);
                }
            }

            // Processar vínculos a remover
            foreach (var vinculo in desvincular)
            {
                const string sqlDelete = @"
                    DELETE FROM ACESSO_USUARIO_EMPRESA_ESTAB
                    WHERE ID_USUARIO = :IdUsuario
                      AND CD_EMPRESA = :CdEmpresa
                      AND CD_ESTABELECIMENTO = :CdEstabelecimento";

                await _session.Connection.ExecuteAsync(
                    sqlDelete,
                    new { IdUsuario = idUsuario, vinculo.CdEmpresa, vinculo.CdEstabelecimento },
                    transaction);
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