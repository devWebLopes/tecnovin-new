# 🏛️ Skill: Oracle Procedures + Dapper — Multi-Cursor & REF CURSOR

## Sobre
Esta skill cobre os padrões avançados de integração Dapper + Oracle para chamar procedures com múltiplos REF CURSORs de saída, funções table-valued e UPSERT — cenários críticos do projeto GestaoNew (DRE, Posição Financeira, Prazo Médio, Compras).

## Estrutura de Constantes — OracleProcedures.cs

Todas as packages/procedures Oracle devem ser centralizadas em constantes. NUNCA usar strings mágicas nos repositories.

```csharp
// Empresa.Data/Oracle/OracleProcedures.cs
namespace Empresa.Data.Oracle;

public static class OracleProcedures
{
    // ---- Compras ----
    public const string SP_REALIZADO = "packageCompraNf.sp_realizado";
    public const string SP_PREVISTO_REALIZADO_ITEM = "packageCompraNf.sp_previsto_realizado_item";
    public const string SP_PROGRESSAO_PRECO = "packageCompraNf.sp_progressao_preco";
    public const string SP_AGRUP_CONTA_CCUSTO = "packageCompraNf.sp_agrup_conta_ccusto";

    // ---- Financeiro ----
    public const string SP_FLUXO_CAIXA = "packageFinanceiro.sp_fluxo_caixa";
    public const string SP_RESUMO_FLUXO = "packageFinanceiro.sp_resumo_geral_flxcxa";
    public const string SP_FLUXO_ANALITICO = "packageFinanceiro.sp_fluxo_caixa_analitico";

    // ---- Posição Financeira (3 versões) ----
    public const string SP_POSICAO_LEGADO = "packageFinanceira.sp_posicao";
    public const string SP_POSICAO_NEW = "packageFinanceiroNew.sp_posicao";
    public const string SP_POSICAO_SREAL = "PKG_POSICAO_FINANCEIRA_SREAL.sp_posicao";

    // ---- DRE (3 versões) ----
    public const string SP_DRE_PADRAO = "packageFinanceiro.sp_fluxo_dre";
    public const string SP_DRE_HOMOLOG = "packageFinanceiroHomolog.sp_fluxo_dre";
    public const string SP_DRE_OUT = "packageFinanceiroOut.sp_fluxo_dre";

    // ---- Prazo Médio ----
    public const string FN_PZM_MENSAL_REC = "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO";
    public const string FN_PZM_PESSOA_REC = "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_PESSOA";
    public const string FN_PZM_DETALHE_REC = "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_DETALHADO";
    public const string FN_PZM_MENSAL_PAG = "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO";
    public const string FN_PZM_PESSOA_PAG = "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_PESSOA";
    public const string FN_PZM_DETALHE_PAG = "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_DETALHADO";

    // ---- Vendas ----
    public const string SP_PAINEL_VENDAS = "packageVendas.sp_painel_vendas_mi";
    public const string SP_COMERCIAL_MI = "packageVenda.sp_comercial_mi";

    // ---- Agrícola ----
    public const string SP_FRUTAS = "packageCompras.sp_recebimento_frutas";
    public const string SP_FRUTAS_DET = "packageCompras.sp_recebimento_frutas_det";

    // ---- DBA ----
    public const string SP_SESSOES = "packageDba.sessoes_agrupadas";
    public const string SP_MATAR_SESSAO = "packageDba.matar_sessao";
}
```

## Padrão 1: Procedure com Múltiplos REF CURSORs (QueryMultiple)

Este é o padrão mais crítico do projeto. Procedures como `sp_realizado` retornam 4 cursores simultaneamente.

```csharp
// Exemplo: sp_realizado retorna 4 cursores:
// Cursor 1: resultado (dados principais)
// Cursor 2: totais (totais agregados)
// Cursor 3: grafico (dados para gráfico)
// Cursor 4: grafico2 (segunda série do gráfico)

public async Task<ResumoAnualComprasResult> GetRealizadoAsync(
    int ano, int idEmpresa, string dataInicio, string dataFim)
{
    var parametros = new DynamicParameters();
    parametros.Add("ANO", ano, DbType.Int32);
    parametros.Add("IDEMPRESA", idEmpresa, DbType.Int32);
    parametros.Add("DATAINICIAL", dataInicio, DbType.String);
    parametros.Add("DATAFINAL", dataFim, DbType.String);

    // Usar ref cursor para Oracle (OracleDynamicParameters)
    parametros.Add("CURSOR_RESULTADO", dbType: DbType.Object,
        direction: ParameterDirection.Output);
    parametros.Add("CURSOR_TOTAIS", dbType: DbType.Object,
        direction: ParameterDirection.Output);
    parametros.Add("CURSOR_GRAFICO", dbType: DbType.Object,
        direction: ParameterDirection.Output);
    parametros.Add("CURSOR_GRAFICO2", dbType: DbType.Object,
        direction: ParameterDirection.Output);

    await using var connection = _session.Connection;
    await connection.OpenAsync();

    // ⚠️ Importante: Oracle requer que a conexão esteja aberta
    // antes de usar QueryMultiple com REF CURSORs
    using var multi = await connection.QueryMultipleAsync(
        OracleProcedures.SP_REALIZADO,
        parametros,
        commandType: CommandType.StoredProcedure
    );

    return new ResumoAnualComprasResult
    {
        Resultado = (await multi.ReadAsync<ResumoAnualCompras>()).ToList(),
        Totais = (await multi.ReadAsync<TotaisCompras>()).ToList(),
        Grafico = (await multi.ReadAsync<GraficoCompras>()).ToList(),
        Grafico2 = (await multi.ReadAsync<GraficoComprasSerie2>()).ToList()
    };
}
```

## Padrão 2: Function Table-Valued (FN_PRAZO_MEDIO_*)

Oracle Functions que retornam tabelas são chamadas via `SELECT * FROM TABLE(PKG.FN_*(...))`.

```csharp
// Exemplo: FN_RECEBIMENTO retorna tabela de prazo médio
public async Task<IEnumerable<PrazoMedioMensal>> GetPrazoMedioRecebimentoAsync(
    int ano, int mes, int idEmpresa)
{
    const string sql = @"
        SELECT * FROM TABLE(
            PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO(:ANO, :MES, :IDEMPRESA)
        )";

    var parametros = new { Ano = ano, Mes = mes, IdEmpresa = idEmpresa };

    return await _session.Connection.QueryAsync<PrazoMedioMensal>(sql, parametros);
}
```

### Drill-Down de 3 Níveis (Prazo Médio)

```csharp
// Nível 1: Mensal (visão agregada por mês)
public async Task<IEnumerable<PrazoMedioMensal>> GetNivel1MensalAsync(
    int ano, int mes, int idEmpresa, string tipo) // tipo = "REC" ou "PAG"
{
    var functionName = tipo == "REC"
        ? "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO"
        : "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO";

    var sql = $"SELECT * FROM TABLE({functionName}(:ANO, :MES, :IDEMPRESA))";
    return await _session.Connection.QueryAsync<PrazoMedioMensal>(sql,
        new { Ano = ano, Mes = mes, IdEmpresa = idEmpresa });
}

// Nível 2: Por Pessoa (drill-down de um mês específico)
public async Task<IEnumerable<PrazoMedioPessoa>> GetNivel2PessoaAsync(
    int ano, int mes, int idEmpresa, string tipo)
{
    var functionName = tipo == "REC"
        ? "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_PESSOA"
        : "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_PESSOA";

    var sql = $"SELECT * FROM TABLE({functionName}(:ANO, :MES, :IDEMPRESA))";
    return await _session.Connection.QueryAsync<PrazoMedioPessoa>(sql,
        new { Ano = ano, Mes = mes, IdEmpresa = idEmpresa });
}

// Nível 3: Documentos (drill-down de uma pessoa específica)
public async Task<IEnumerable<PrazoMedioDocumento>> GetNivel3DocumentosAsync(
    int ano, int mes, int idEmpresa, int idPessoa, string tipo)
{
    var functionName = tipo == "REC"
        ? "PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_DETALHADO"
        : "PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_DETALHADO";

    var sql = $"SELECT * FROM TABLE({functionName}(:ANO, :MES, :IDEMPRESA, :IDPESSOA))";
    return await _session.Connection.QueryAsync<PrazoMedioDocumento>(sql,
        new { Ano = ano, Mes = mes, IdEmpresa = idEmpresa, IdPessoa = idPessoa });
}
```

## Padrão 3: Procedure com Parâmetros de Saída Simples

```csharp
public async Task<int> CreateUsuarioAsync(Usuario usuario)
{
    const string sql = @"
        INSERT INTO USUARIO (NOME, LOGIN, SENHA, ATIVO)
        VALUES (:Nome, :Login, :Senha, :Ativo)
        RETURNING ID_USUARIO INTO :Id";

    var parametros = new DynamicParameters(usuario);
    parametros.Add("Id", dbType: DbType.Int32,
        direction: ParameterDirection.ReturnValue);

    await _session.Connection.ExecuteAsync(sql, parametros);
    return parametros.Get<int>("Id");
}
```

## Padrão 4: UPSERT (MERGE)

```csharp
public async Task UpsertAcessoPaginaAsync(int idUsuario, int idPagina)
{
    const string sql = @"
        MERGE INTO ACESSO_VISUALIZACAO_PAGINA AVP
        USING (SELECT :IdUsuario AS ID_USUARIO, :IdPagina AS ID_PAGINA FROM DUAL) SRC
        ON (AVP.ID_USUARIO = SRC.ID_USUARIO AND AVP.ID_PAGINA = SRC.ID_PAGINA)
        WHEN MATCHED THEN
            UPDATE SET QUANTIDADE_ACESSO = QUANTIDADE_ACESSO + 1,
                       DT_ULTIMO_ACESSO = SYSDATE
        WHEN NOT MATCHED THEN
            INSERT (ID_USUARIO, ID_PAGINA, QUANTIDADE_ACESSO, DT_ULTIMO_ACESSO)
            VALUES (SRC.ID_USUARIO, SRC.ID_PAGINA, 1, SYSDATE)";

    await _session.Connection.ExecuteAsync(sql,
        new { IdUsuario = idUsuario, IdPagina = idPagina });
}
```

## Padrão 5: Execução de Procedure sem Retorno (Fire-and-Forget)

```csharp
public async Task SalvaAcessoUsuarioAsync(int idUsuario)
{
    const string sql = @"
        UPDATE USUARIO
        SET QUANTIDADE_ACESSO = NVL(QUANTIDADE_ACESSO, 0) + 1,
            DT_ULTIMO_ACESSO = SYSDATE
        WHERE ID_USUARIO = :IdUsuario";

    await _session.Connection.ExecuteAsync(sql,
        new { IdUsuario = idUsuario });
}
```

## Padrão 6: Query com CONNECT BY (Hierarquia)

```csharp
public async Task<IEnumerable<Pagina>> GetMenuHierarquicoAsync(int idPerfil)
{
    const string sql = @"
        SELECT ID_PAGINA, TITULO_MENU, URL, ID_PAGINA_PAI, ORDEM, LEVEL
        FROM PAGINA P
        WHERE P.ATIVO = 'S'
          AND EXISTS (
              SELECT 1 FROM PERFIL_PAGINA PP
              WHERE PP.ID_PAGINA = P.ID_PAGINA
                AND PP.ID_PERFIL = :IdPerfil
          )
        START WITH P.ID_PAGINA_PAI IS NULL
        CONNECT BY PRIOR P.ID_PAGINA = P.ID_PAGINA_PAI
        ORDER SIBLINGS BY ORDEM";

    return await _session.Connection.QueryAsync<Pagina>(sql,
        new { IdPerfil = idPerfil });
}
```

## Regras Obrigatórias

1. **SEMPRE** usar constantes em `OracleProcedures.cs` — NUNCA strings mágicas com nome de package
2. **SEMPRE** usar `DynamicParameters` para Oracle — bind variables com prefixo `:`
3. **SEMPRE** abrir conexão (`OpenAsync()`) antes de `QueryMultiple` com REF CURSORs
4. **SEMPRE** usar `CommandType.StoredProcedure` para procedures e packages
5. **SEMPRE** usar `CommandType.Text` para funções table-valued (`SELECT * FROM TABLE(...)`)
6. **SEMPRE** usar `NVL()` para tratar NULLs do Oracle (ex: `NVL(QUANTIDADE_ACESSO, 0)`)
7. **SEMPRE** usar `SYSDATE` para data/hora atual (nunca `GETDATE()` do SQL Server)
8. **SEMPRE** usar `RETURNING ... INTO :Param` para obter IDs gerados
9. **NUNCA** concatenar strings SQL com valores de usuário — sempre `DynamicParameters`
10. **SEMPRE** usar `await using` para conexões e `using` para `QueryMultiple`

## Anti-Padrões (Proibidos)

```csharp
// ❌ NUNCA usar string mágica para nome de package
var sql = "packageFinanceiro.sp_fluxo_caixa"; // Use OracleProcedures.SP_FLUXO_CAIXA

// ❌ NUNCA esquecer de abrir conexão antes de QueryMultiple
using var multi = await connection.QueryMultipleAsync(...); // connection não está aberta!

// ❌ NUNCA usar @ para parâmetros Oracle
new { @Id = 1 }  // SQL Server syntax → Oracle usa :Id

// ❌ NUNCA usar TOP (SQL Server) em Oracle
SELECT TOP 10 * FROM USUARIO  // Use FETCH FIRST 10 ROWS ONLY

// ❌ NUNCA esquecer de tratar NULL do Oracle
SELECT QUANTIDADE_ACESSO + 1 FROM USUARIO  // Se NULL, resultado é NULL! Use NVL()
```

## Tratamento de 7 Tipos de Operação PZM Pagamento

```csharp
public enum TipoOperacaoPZM
{
    G = 'G', // Geral
    I = 'I', // Industrialização
    O = 'O', // Operação
    U = 'U', // Unidade
    L = 'L', // Local
    M = 'M', // Matriz
    S = 'S'  // Sede
}

public async Task<IEnumerable<PrazoMedioMensal>> GetPZMPorOperacaoAsync(
    int ano, int mes, int idEmpresa, TipoOperacaoPZM operacao)
{
    var sql = @"SELECT * FROM TABLE(
        PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO(:ANO, :MES, :IDEMPRESA, :OPERACAO))";

    return await _session.Connection.QueryAsync<PrazoMedioMensal>(sql,
        new { Ano = ano, Mes = mes, IdEmpresa = idEmpresa,
              Operacao = operacao.ToString() });
}
```

## Pós-Processamento na Camada de Serviço

Dados que exigem transformação (ex: remoção de colunas vazias) DEVEM ser processados na camada de serviço, NUNCA no repositório.

```csharp
// No Service (Empresa.Api/Services/FinanceiroService.cs)
public async Task<PosicaoFinanceiraResponse> GetPosicaoSemanalAsync(FiltroRequest filtro)
{
    // 1. Obtém dados brutos do repositório
    var dadosBrutos = await _repo.GetPosicaoSemanalAsync(filtro);

    // 2. Pós-processamento: remove colunas de semanas sem dados
    var colunasComDados = dadosBrutos.Semanas
        .Where(s => s.Valor != 0)
        .ToList();

    return new PosicaoFinanceiraResponse
    {
        Semanas = colunasComDados,
        Total = colunasComDados.Sum(s => s.Valor)
    };
}