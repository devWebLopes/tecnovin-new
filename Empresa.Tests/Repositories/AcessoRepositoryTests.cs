using Dapper;
using Empresa.Data;
using Empresa.Data.Repositories;
using Empresa.Tests.Helpers;

namespace Empresa.Tests.Repositories;

public class AcessoRepositoryTests
{
    static AcessoRepositoryTests()
    {
        // Mesma configuração do Program.cs da API (mapeamento de colunas Oracle).
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private static AcessoRepository CriarRepositorio(FakeDbConnection connection)
        => new(new DbSession(connection));

    // ─── QA-01: CONNECT BY (pais implícitos, ≥3 níveis) — contrato SQL + mapeamento

    [Fact]
    public async Task GetPaginasMenuAsync_IssuesConnectBySql_WithPerfilParameter()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("CONNECT BY PRIOR")
                ? () => new FakeDbReader(
                    new[] { "ID_PAGINA", "URL", "TITULO_ABA", "CHAVE_CONTROLE", "TITULO_MENU", "ID_PAGINA_PAI", "ORDEM", "TOOLTIP", "ATIVO" },
                    new[]
                    {
                        new object?[] { 1, "/cadastros", "Cadastros", "cadastros", "Cadastros", null, 1, "tooltip", "S" },
                        new object?[] { 2, "/cadastros/usuarios", "Usuários", "usuarios", "Usuários", 1, 1, "tooltip", "S" },
                        new object?[] { 3, "/cadastros/usuarios/novo", "Novo Usuário", "novo-usuario", "Novo Usuário", 2, 1, "tooltip", "S" }
                    })
                : null);

        var repo = CriarRepositorio(connection);
        var resultado = (await repo.GetPaginasMenuAsync(10)).ToList();

        Assert.Equal(3, resultado.Count);
        var cmd = connection.Commands.Single(c => c.CommandText.Contains("CONNECT BY PRIOR"));
        Assert.Contains("START WITH", cmd.CommandText);
        Assert.Contains("ACESSO_PERFIL_PAGINA", cmd.CommandText);
        Assert.Contains("CONNECT BY PRIOR", cmd.CommandText);
        Assert.Contains("p.ATIVO = 'S'", cmd.CommandText);
        var param = Assert.IsType<FakeDbParameter>(cmd.Parameters["IdPerfil"]);
        Assert.Equal(10, param.Value);

        Assert.Null(resultado[0].IDPaginaPai);
        Assert.Equal(1, resultado[1].IDPaginaPai);
        Assert.Equal("Usuários", resultado[1].TituloMenu);
        Assert.Equal(2, resultado[2].IDPaginaPai);
    }

    // ─── QA-01: mais acessados — filtro perfil, limite 10, ordenação QUANTIDADE DESC

    [Fact]
    public async Task GetPaginasMaisAcessadasAsync_FiltersByPerfil_Limits10_OrdersByQuantidadeDesc()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("FETCH FIRST 10 ROWS ONLY")
                ? () => new FakeDbReader(
                    new[] { "ID_PAGINA", "URL", "TITULO_ABA", "CHAVE_CONTROLE", "TITULO_MENU", "ID_PAGINA_PAI", "ORDEM", "TOOLTIP", "ATIVO", "QUANTIDADE" },
                    new[]
                    {
                        new object?[] { 1, "/inicio", "Início", "inicio", "Início", null, 1, "", "S", 42 },
                        new object?[] { 2, "/vendas", "Vendas", "vendas", "Vendas", null, 2, "", "S", 17 }
                    })
                : null);

        var repo = CriarRepositorio(connection);
        var resultado = (await repo.GetPaginasMaisAcessadasAsync(5, 10)).ToList();

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("FETCH FIRST 10 ROWS ONLY"));
        Assert.Contains("ACESSO_VISUALIZACAO_PAGINA", cmd.CommandText);
        Assert.Contains("INNER JOIN ACESSO_PERFIL_PAGINA", cmd.CommandText);
        Assert.Contains("avp.ID_USUARIO = :IdUsuario", cmd.CommandText);
        Assert.Contains("pp.ID_PERFIL = :IdPerfil", cmd.CommandText);
        Assert.Contains("p.ATIVO = 'S'", cmd.CommandText);
        Assert.Contains("ORDER BY avp.QUANTIDADE DESC", cmd.CommandText);
        Assert.Contains("FETCH FIRST 10 ROWS ONLY", cmd.CommandText);

        Assert.Equal(5, Assert.IsType<FakeDbParameter>(cmd.Parameters["IdUsuario"]).Value);
        Assert.Equal(10, Assert.IsType<FakeDbParameter>(cmd.Parameters["IdPerfil"]).Value);

        Assert.Equal(42, resultado[0].Quantidade);
        Assert.Equal("Início", resultado[0].TituloMenu);
        Assert.Equal("Vendas", resultado[1].TituloMenu);
    }

    // ─── QA-04: telemetria — MERGE upsert com vínculo perfil×página

    [Fact]
    public async Task GetPaginaByChaveAsync_ChecksPermissionJoin()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("CHAVE_CONTROLE = :ChaveControle")
                ? () => new FakeDbReader(
                    new[] { "ID_PAGINA", "URL", "TITULO_ABA", "CHAVE_CONTROLE", "TITULO_MENU", "ID_PAGINA_PAI", "ATIVO", "ORDEM", "TOOLTIP" },
                    new[]
                    {
                        new object?[] { 42, "/usuarios", "Usuários", "usuarios", "Usuários", null, "S", 1, "" }
                    })
                : null);

        var repo = CriarRepositorio(connection);
        var pagina = await repo.GetPaginaByChaveAsync("usuarios", 10);

        Assert.NotNull(pagina);
        Assert.Equal(42, pagina!.IDPagina);

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("CHAVE_CONTROLE = :ChaveControle"));
        Assert.Contains("INNER JOIN ACESSO_PERFIL_PAGINA APP", cmd.CommandText);
        Assert.Contains("APP.ID_PERFIL = :IdPerfil", cmd.CommandText);
        Assert.Contains("ROWNUM = 1", cmd.CommandText);
        Assert.Equal("usuarios", Assert.IsType<FakeDbParameter>(cmd.Parameters["ChaveControle"]).Value);
        Assert.Equal(10, Assert.IsType<FakeDbParameter>(cmd.Parameters["IdPerfil"]).Value);
    }

    [Fact]
    public async Task RegistraAcessoPaginaAsync_IssuesMergeUpsert()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("MERGE INTO ACESSO_VISUALIZACAO_PAGINA")
                ? () => new FakeDbReader(Array.Empty<string>(), Array.Empty<object?[]>())
                : null);

        var repo = CriarRepositorio(connection);
        await repo.RegistraAcessoPaginaAsync(5, 42);

        var cmd = connection.Commands.Single(c => c.CommandText.Contains("MERGE INTO ACESSO_VISUALIZACAO_PAGINA"));
        var sql = cmd.CommandText.Replace("\r", " ").Replace("\n", " ");

        Assert.Contains("WHEN MATCHED THEN", sql);
        Assert.Contains("UPDATE SET QUANTIDADE = QUANTIDADE + 1", sql);
        Assert.Contains("WHEN NOT MATCHED THEN", sql);
        Assert.Contains("INSERT", sql);
        Assert.Equal(5, Assert.IsType<FakeDbParameter>(cmd.Parameters["IdUsuario"]).Value);
        Assert.Equal(42, Assert.IsType<FakeDbParameter>(cmd.Parameters["IdPagina"]).Value);
    }

    // ─── RN-09: usuário do menu — contagem de vínculos B.I.

    [Fact]
    public async Task GetMenuUsuarioAsync_ReturnsVinculos()
    {
        var connection = new FakeDbConnection(sql =>
            sql.Contains("USUARIO_EMPRESA")
                ? () => new FakeDbReader(
                    new[] { "ID_USUARIO", "NOME", "LOGIN", "VINCULOS" },
                    new[] { new object?[] { 5, "João", "joao", 3 } })
                : null);

        var repo = CriarRepositorio(connection);
        var usuario = await repo.GetMenuUsuarioAsync(5);

        Assert.NotNull(usuario);
        Assert.Equal(3, usuario!.Vinculos);
        Assert.Equal("João", usuario.Nome);
    }
}
