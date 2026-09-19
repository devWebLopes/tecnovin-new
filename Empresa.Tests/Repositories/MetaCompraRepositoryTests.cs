using Dapper;
using Empresa.Data;
using Empresa.Data.Repositories;
using Empresa.Tests.Helpers;

namespace Empresa.Tests.Repositories;

public class MetaCompraRepositoryTests
{
    static MetaCompraRepositoryTests()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private static MetaCompraRepository CriarRepositorio(FakeDbConnection connection)
        => new(new DbSession(connection));

    [Fact]
    public async Task GetAllAsync_IssuesCorrectSql_OrdersBySafraDesc()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("META_COMPRAS") && sql.Contains("ORDER BY SAFRA DESC")
                ? () => new FakeDbReader(
                    new[] { "ID_META_COMPRA", "CD_LINHA", "SAFRA", "DT_INICIAL", "DT_FINAL", "META_QTDE", "CD_EMPRESA" },
                    new[]
                    {
                        new object?[] { 1024L, 10L, "2026", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1500000m, 2L },
                        new object?[] { 1023L, 5L, "2025", new DateTime(2025, 1, 1), new DateTime(2025, 12, 31), 1200000m, 200L }
                    })
                : null);

        var repo = CriarRepositorio(connection);
        var result = (await repo.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("2026", result[0].Safra);
        Assert.Equal("2025", result[1].Safra);

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("META_COMPRAS") && c.CommandText.Contains("ORDER BY"));
        Assert.Contains("ID_META_COMPRA", cmd.CommandText);
        Assert.Contains("CD_LINHA", cmd.CommandText);
        Assert.Contains("SAFRA DESC", cmd.CommandText);
    }

    [Fact]
    public async Task GetByIdAsync_IssuesSqlWithWhereId()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("WHERE ID_META_COMPRA =")
                ? () => new FakeDbReader(
                    new[] { "ID_META_COMPRA", "CD_LINHA", "SAFRA", "DT_INICIAL", "DT_FINAL", "META_QTDE", "CD_EMPRESA" },
                    new[] { new object?[] { 1024L, 10L, "2026", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 1500000m, 2L } })
                : null);

        var repo = CriarRepositorio(connection);
        var result = await repo.GetByIdAsync(1024);

        Assert.NotNull(result);
        Assert.Equal(1024, result.IdMetaCompra);

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("WHERE ID_META_COMPRA"));
        var param = Assert.IsType<FakeDbParameter>(cmd.Parameters["IdMetaCompra"]);
        Assert.Equal(1024L, param.Value);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var connection = new FakeDbConnection(_ => null);
        var repo = CriarRepositorio(connection);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task InsertAsync_IssuesInsertWithReturning()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("INSERT INTO META_COMPRAS")
                ? () => new FakeDbReader(new[] { "ID_META_COMPRA" }, new[] { new object?[] { 1025L } })
                : null);

        var repo = CriarRepositorio(connection);
        var meta = new Empresa.Data.Models.MetaCompra
        {
            CdLinha = 10,
            Safra = "2026",
            DtInicial = new DateTime(2026, 1, 1),
            DtFinal = new DateTime(2026, 12, 31),
            MetaQtde = 1500000,
            CdEmpresa = 2
        };

        // FakeDbConnection doesn't support OUTPUT parameters, so we test the SQL contract
        try { await repo.InsertAsync(meta); } catch { }

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("INSERT INTO META_COMPRAS"));
        Assert.Contains("RETURNING", cmd.CommandText);
        Assert.Contains("CD_LINHA", cmd.CommandText);
        Assert.Contains("CD_EMPRESA", cmd.CommandText);
    }

    [Fact]
    public async Task UpdateAsync_IssuesUpdateWithWhereId()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("UPDATE META_COMPRAS")
                ? () => new FakeDbReader(new[] { "N" }, new[] { new object?[] { 1 } })
                : null);

        var repo = CriarRepositorio(connection);
        var meta = new Empresa.Data.Models.MetaCompra
        {
            IdMetaCompra = 1024,
            CdLinha = 10,
            Safra = "2026",
            DtInicial = new DateTime(2026, 1, 1),
            DtFinal = new DateTime(2026, 12, 31),
            MetaQtde = 1500000,
            CdEmpresa = 2
        };

        var result = await repo.UpdateAsync(meta);

        Assert.True(result);
        var cmd = connection.Commands.Single(c => c.CommandText.Contains("UPDATE META_COMPRAS"));
        Assert.Contains("WHERE ID_META_COMPRA", cmd.CommandText);
    }

    [Fact]
    public async Task DeleteAsync_IssuesDeleteWithWhereId()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("DELETE FROM META_COMPRAS")
                ? () => new FakeDbReader(new[] { "N" }, new[] { new object?[] { 1 } })
                : null);

        var repo = CriarRepositorio(connection);

        var result = await repo.DeleteAsync(1024);

        Assert.True(result);
        var cmd = connection.Commands.Single(c => c.CommandText.Contains("DELETE FROM META_COMPRAS"));
        var param = Assert.IsType<FakeDbParameter>(cmd.Parameters["IdMetaCompra"]);
        Assert.Equal(1024L, param.Value);
    }
}
