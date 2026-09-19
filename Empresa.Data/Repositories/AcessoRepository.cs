using Dapper;
using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de acesso - autenticação, menu e páginas
/// </summary>
public class AcessoRepository : IAcessoRepository
{
    private readonly DbSession _session;

    public AcessoRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<Usuario?> GetUsuarioAsync(string login, string senhaHash)
    {
        const string sql = @"
            SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL,
                   QUANTIDADE_ACESSO, ATUALIZA_SENHA, SENHA, ATIVO
            FROM ACESSO_CADASTRO_USUARIO
            WHERE LOGIN = :Login
              AND SENHA = :Senha
              AND ATIVO = 'S'";

        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
            sql, new { Login = login, Senha = senhaHash });
    }

    public async Task<Usuario?> GetUsuarioByLoginAsync(string login)
    {
        const string sql = @"
            SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL,
                   QUANTIDADE_ACESSO, ATUALIZA_SENHA, SENHA, ATIVO
            FROM ACESSO_CADASTRO_USUARIO
            WHERE LOGIN = :Login
              AND ATIVO = 'S'";

        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
            sql, new { Login = login });
    }

    public async Task SalvaAcessoUsuarioAsync(int idUsuario)
    {
        const string sql = @"
            UPDATE ACESSO_CADASTRO_USUARIO
            SET QUANTIDADE_ACESSO = QUANTIDADE_ACESSO + 1
            WHERE ID_USUARIO = :Id";

        await _session.Connection.ExecuteAsync(sql, new { Id = idUsuario });
    }

    public async Task<IEnumerable<Pagina>> GetPaginasMenuAsync(int idPerfil)
    {
        // CONNECT BY PRIOR reproduzindo o UNION legado (SQL 5.1 do menus.md):
        //   START WITH  → páginas com vínculo direto no perfil (RN-01)
        //   CONNECT BY  → sobe a hierarquia trazendo TODOS os ancestrais (pais implícitos RN-03),
        //                 com profundidade ilimitada (D-07)
        //   WHERE p.ATIVO='S' → aplicado a folhas E ancestrais (correção D-06)
        const string sql = @"
            SELECT DISTINCT p.ID_PAGINA, p.URL, p.TITULO_ABA, p.CHAVE_CONTROLE,
                   p.TITULO_MENU, p.ID_PAGINA_PAI, p.ORDEM, p.TOOLTIP, p.ATIVO
              FROM ACESSO_CADASTRO_PAGINA p
             WHERE p.ATIVO = 'S'
               AND p.ID_PAGINA IN (
                     SELECT q.ID_PAGINA
                       FROM ACESSO_CADASTRO_PAGINA q
                      START WITH q.ID_PAGINA IN (
                                 SELECT pp.ID_PAGINA
                                   FROM ACESSO_PERFIL_PAGINA pp
                                  WHERE pp.ID_PERFIL = :IdPerfil)
                      CONNECT BY PRIOR q.ID_PAGINA_PAI = q.ID_PAGINA)
             ORDER BY p.ORDEM";

        return await _session.Connection.QueryAsync<Pagina>(sql, new { IdPerfil = idPerfil });
    }

    public async Task<Pagina?> GetPaginaByChaveAsync(string chaveControle, int idPerfil)
    {
        const string sql = @"
            SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
                   ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
              FROM ACESSO_CADASTRO_PAGINA ACP
             INNER JOIN ACESSO_PERFIL_PAGINA APP ON APP.ID_PAGINA = ACP.ID_PAGINA
             WHERE ACP.CHAVE_CONTROLE = :ChaveControle
               AND APP.ID_PERFIL = :IdPerfil
               AND ACP.ATIVO = 'S'
               AND ROWNUM = 1";

        return await _session.Connection.QueryFirstOrDefaultAsync<Pagina>(
            sql, new { ChaveControle = chaveControle, IdPerfil = idPerfil });
    }

    public async Task<IEnumerable<PaginaAcesso>> GetPaginasMaisAcessadasAsync(int idUsuario, int idPerfil)
    {
        // SQL 5.2 do legado corrigido: JOIN com ACESSO_PERFIL_PAGINA (D-05 — página revogada
        // não aparece), ATIVO='S' (RN-02), ordenação explícita por QUANTIDADE DESC (D-04)
        // e limite de 10 (RN-07).
        const string sql = @"
            SELECT p.ID_PAGINA, p.URL, p.TITULO_ABA, p.CHAVE_CONTROLE,
                   p.TITULO_MENU, p.ID_PAGINA_PAI, p.ORDEM, p.TOOLTIP, p.ATIVO,
                   avp.QUANTIDADE
              FROM ACESSO_VISUALIZACAO_PAGINA avp
             INNER JOIN ACESSO_CADASTRO_PAGINA p ON avp.ID_PAGINA = p.ID_PAGINA
             INNER JOIN ACESSO_PERFIL_PAGINA pp ON pp.ID_PAGINA = p.ID_PAGINA
             WHERE avp.ID_USUARIO = :IdUsuario
               AND pp.ID_PERFIL = :IdPerfil
               AND p.ATIVO = 'S'
             ORDER BY avp.QUANTIDADE DESC
             FETCH FIRST 10 ROWS ONLY";

        return await _session.Connection.QueryAsync<PaginaAcesso>(
            sql, new { IdUsuario = idUsuario, IdPerfil = idPerfil });
    }

    public async Task<MenuUsuarioInfo?> GetMenuUsuarioAsync(int idUsuario)
    {
        // SQL 5.5 do legado: contagem de vínculos em USUARIO_EMPRESA decide o link do B.I. (RN-09)
        const string sql = @"
            SELECT U.ID_USUARIO, U.NOME, U.LOGIN,
                   (SELECT COUNT(1) FROM USUARIO_EMPRESA BI
                     WHERE BI.ID_USUARIO = U.ID_USUARIO) AS VINCULOS
              FROM ACESSO_CADASTRO_USUARIO U
             WHERE U.ID_USUARIO = :IdUsuario";

        return await _session.Connection.QueryFirstOrDefaultAsync<MenuUsuarioInfo>(
            sql, new { IdUsuario = idUsuario });
    }

    public async Task RegistraAcessoPaginaAsync(int idUsuario, int idPagina)
    {
        const string sql = @"
            MERGE INTO ACESSO_VISUALIZACAO_PAGINA avp
            USING (SELECT :IdUsuario AS ID_USUARIO, :IdPagina AS ID_PAGINA FROM DUAL) src
            ON (avp.ID_USUARIO = src.ID_USUARIO AND avp.ID_PAGINA = src.ID_PAGINA)
            WHEN MATCHED THEN
                UPDATE SET QUANTIDADE = QUANTIDADE + 1, DATA_HORA = SYSDATE
            WHEN NOT MATCHED THEN
                INSERT (ID_USUARIO, ID_PAGINA, QUANTIDADE, DATA_HORA)
                VALUES (src.ID_USUARIO, src.ID_PAGINA, 1, SYSDATE)";

        await _session.Connection.ExecuteAsync(sql, new { IdUsuario = idUsuario, IdPagina = idPagina });
    }

    public async Task<IEnumerable<Estabelecimento>> GetEstabelecimentosTreeAsync()
    {
        const string sql = @"
            SELECT ID_ESTABELECIMENTO, NOME, CNPJ, ATIVO
            FROM VW_ESTABELECIMENTO_NEW
            WHERE ATIVO = 'S'
            ORDER BY NOME";

        return await _session.Connection.QueryAsync<Estabelecimento>(sql);
    }

    public async Task RegistraRelatorioAsync(int idUsuario, string tipoRelatorio, string? ip = null)
    {
        // INSERT em REGISTRO_RELATORIOS conforme legado (seção 11.8 do doc_legado)
        // Campos: ID_USUARIO, TIPO_RELATORIO, DT_GERACAO, IP
        const string sql = @"
            INSERT INTO REGISTRO_RELATORIOS
                (ID_USUARIO, TIPO_RELATORIO, DT_GERACAO, IP)
            VALUES
                (:IdUsuario, :TipoRelatorio, SYSDATE, :Ip)";

        await _session.Connection.ExecuteAsync(sql, new
        {
            IdUsuario = idUsuario,
            TipoRelatorio = tipoRelatorio,
            Ip = ip ?? "N/A"
        });
    }
}
