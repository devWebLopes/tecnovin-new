using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de estabelecimentos — CRUD com exclusão lógica
/// </summary>
public class EstabelecimentoRepository : IEstabelecimentoRepository
{
    private readonly DbSession _session;

    public EstabelecimentoRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Estabelecimento>> GetAllAsync()
    {
        const string sql = @"
            SELECT ID_ESTABELECIMENTO, NOME, CNPJ, ATIVO
            FROM ESTABELECIMENTO
            ORDER BY NOME";

        return await _session.Connection.QueryAsync<Estabelecimento>(sql);
    }

    public async Task<Estabelecimento?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT ID_ESTABELECIMENTO, NOME, CNPJ, ATIVO
            FROM ESTABELECIMENTO
            WHERE ID_ESTABELECIMENTO = :Id";

        return await _session.Connection.QueryFirstOrDefaultAsync<Estabelecimento>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Estabelecimento estabelecimento)
    {
        const string sql = @"
            INSERT INTO ESTABELECIMENTO (NOME, CNPJ, ATIVO)
            VALUES (:Nome, :Cnpj, :Ativo)
            RETURNING ID_ESTABELECIMENTO INTO :Id";

        var parameters = new DynamicParameters(new { estabelecimento.Nome, estabelecimento.Cnpj, estabelecimento.Ativo });
        parameters.Add(":Id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> UpdateAsync(Estabelecimento estabelecimento)
    {
        const string sql = @"
            UPDATE ESTABELECIMENTO
            SET NOME = :Nome,
                CNPJ = :Cnpj,
                ATIVO = :Ativo
            WHERE ID_ESTABELECIMENTO = :IDEstabelecimento";

        var rows = await _session.Connection.ExecuteAsync(sql, estabelecimento);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE ESTABELECIMENTO
            SET ATIVO = 'N'
            WHERE ID_ESTABELECIMENTO = :Id";

        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}