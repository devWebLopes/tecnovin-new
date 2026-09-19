# 🗄️ Camada de Dados — GestãoNew

## Visão Geral

A camada de dados (`Empresa.Data`) é responsável por toda comunicação com o banco Oracle, utilizando **Dapper** como ORM e **Oracle.ManagedDataAccess.Core** como driver.

---

## Tecnologias

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| Dapper | 2.1.79 | Micro-ORM para mapeamento objeto-relacional |
| Oracle.ManagedDataAccess.Core | 23.26.300 | Driver gerenciado Oracle para .NET |
| Oracle Database | 23c | Banco de dados relacional |

---

## DbSession — Gerenciamento de Conexão

O **DbSession** é responsável por gerenciar o ciclo de vida da conexão com o Oracle. Deve ser registrado como **Scoped** (uma conexão por requisição).

### Implementação

```csharp
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Empresa.Data;

/// <summary>
/// Gerencia a conexão com Oracle via Dapper
/// </summary>
public class DbSession : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public DbSession(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Retorna a conexão ativa (cria se não existir)
    /// </summary>
    public IDbConnection Connection =>
        _connection ??= new OracleConnection(_connectionString);

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
```

### Registro no Program.cs

```csharp
// Empresa.Api/Program.cs ou Empresa.Worker/Program.cs
builder.Services.AddScoped<DbSession>(sp =>
    new DbSession(builder.Configuration.GetConnectionString("Oracle") ?? string.Empty));
```

---

## Padrão Repository

### Interface

```csharp
namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de usuários
/// </summary>
public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task<int> CreateAsync(Usuario usuario);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int id);
    Task<bool> LoginExistsAsync(string login);
}
```

### Implementação com Dapper

```csharp
namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de usuários com acesso Oracle via Dapper
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbSession _session;

    public UsuarioRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        const string sql = @"
            SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL, 
                   QUANTIDADE_ACESSO, ATUALIZA_SENHA, ATIVO
            FROM USUARIO 
            WHERE ATIVO = 'S'
            ORDER BY NOME";

        return await _session.Connection.QueryAsync<Usuario>(sql);
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL,
                   QUANTIDADE_ACESSO, ATUALIZA_SENHA, ATIVO
            FROM USUARIO 
            WHERE ID_USUARIO = :Id";

        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
            sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        const string sql = @"
            INSERT INTO USUARIO (NOME, LOGIN, SENHA, ID_PERFIL, ATIVO)
            VALUES (:Nome, :Login, :Senha, :IDPerfil, :Ativo)
            RETURNING ID_USUARIO INTO :Id";

        var parameters = new DynamicParameters(usuario);
        parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        const string sql = @"
            UPDATE USUARIO 
            SET NOME = :Nome, 
                LOGIN = :Login, 
                ID_PERFIL = :IDPerfil, 
                ATIVO = :Ativo
            WHERE ID_USUARIO = :IDUsuario";

        var rows = await _session.Connection.ExecuteAsync(sql, usuario);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Exclusão lógica (padrão do projeto)
        const string sql = @"
            UPDATE USUARIO 
            SET ATIVO = 'N' 
            WHERE ID_USUARIO = :Id";

        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> LoginExistsAsync(string login)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM USUARIO 
            WHERE LOGIN = :Login AND ATIVO = 'S'";

        var count = await _session.Connection.ExecuteScalarAsync<int>(sql, new { Login = login });
        return count > 0;
    }
}
```

---

## Models (Entidades)

### Regras para Models

- Mapeamento **direto** para as colunas da tabela Oracle
- Propriedades em PascalCase (Dapper mapeia automaticamente)
- Valores padrão para evitar nulls: `= string.Empty`, `= "S"`
- Nomes de propriedades seguem o padrão Oracle (ex: `IDUsuario` → coluna `ID_USUARIO`)

### Exemplo

```csharp
namespace Empresa.Data.Models;

/// <summary>
/// Representa um usuário do sistema — mapeia tabela USUARIO
/// </summary>
public class Usuario
{
    public int IDUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public int IDPerfil { get; set; }
    public int QuantidadeAcesso { get; set; }
    public string AtualizaSenha { get; set; } = "N";
    public string Senha { get; set; } = string.Empty;
    public string Ativo { get; set; } = "S";
}
```

---

## Queries Dapper — Oracle

### Diferenças Oracle vs SQL Server

| Operação | SQL Server | Oracle |
|----------|-----------|--------|
| Parâmetros | `@nome` | `:nome` |
| Paginação | `OFFSET ... FETCH NEXT` | `OFFSET ... FETCH NEXT` (Oracle 12c+) |
| Data atual | `GETDATE()` | `SYSDATE` |
| Top N | `SELECT TOP 10` | `FETCH FIRST 10 ROWS ONLY` |
| Identity | `IDENTITY` | `GENERATED BY DEFAULT AS IDENTITY` |
| String vazia | `''` | `NULL` (Oracle trata como NULL) |
| Concatenação | `+` | `\|\|` ou `CONCAT()` |

### Padrões de Query

#### SELECT com parâmetros
```csharp
const string sql = "SELECT * FROM USUARIO WHERE ID_USUARIO = :Id";
return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
```

#### INSERT com RETURNING (obter ID gerado)
```csharp
const string sql = @"
    INSERT INTO USUARIO (NOME, LOGIN, SENHA, ATIVO)
    VALUES (:Nome, :Login, :Senha, :Ativo)
    RETURNING ID_USUARIO INTO :Id";

var parameters = new DynamicParameters(new { usuario.Nome, usuario.Login, usuario.Senha, usuario.Ativo });
parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
await _session.Connection.ExecuteAsync(sql, parameters);
return parameters.Get<int>(":Id");
```

#### UPDATE
```csharp
const string sql = @"
    UPDATE USUARIO 
    SET NOME = :Nome, LOGIN = :Login 
    WHERE ID_USUARIO = :IDUsuario";

var rows = await _session.Connection.ExecuteAsync(sql, usuario);
```

#### LIKE com bind variables (Oracle)
```csharp
const string sql = @"
    SELECT * FROM USUARIO 
    WHERE UPPER(NOME) LIKE CONCAT('%', CONCAT(UPPER(:Termo), '%'))";

return await _session.Connection.QueryAsync<Usuario>(sql, new { Termo = termoBusca });
```

#### Paginação
```csharp
const string sql = @"
    SELECT ID_USUARIO, NOME, LOGIN
    FROM USUARIO
    WHERE ATIVO = 'S'
    ORDER BY NOME
    OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

return await _session.Connection.QueryAsync<Usuario>(sql, new { Offset = (page - 1) * pageSize, PageSize = pageSize });
```

#### Total Count para paginação
```csharp
const string sql = "SELECT COUNT(1) FROM USUARIO WHERE ATIVO = 'S'";
var total = await _session.Connection.ExecuteScalarAsync<int>(sql);
```

---

## DynamicParameters

Para consultas que precisam de parâmetros de saída ou tipos específicos:

```csharp
var parameters = new DynamicParameters();

// Entrada
parameters.Add(":Nome", usuario.Nome, DbType.String);
parameters.Add(":Login", usuario.Login, DbType.String);

// Saída (RETURNING)
parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

await _session.Connection.ExecuteAsync(sql, parameters);
var id = parameters.Get<int>(":Id");
```

---

## Transações

Para operações que exigem atomicidade:

```csharp
using var transaction = _session.Connection.BeginTransaction();

try
{
    await _session.Connection.ExecuteAsync(sql1, parameters1, transaction);
    await _session.Connection.ExecuteAsync(sql2, parameters2, transaction);

    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## Boas Práticas

### ✅ Faça
- Use `const string sql` para queries estáticas
- Sempre especifique colunas (nunca `SELECT *`)
- Use parâmetros (`new { ... }` ou `DynamicParameters`)
- Trate conexões com `using` ou `await using`
- Implemente exclusão lógica (coluna `ATIVO = 'N'`)
- Use `ORDER BY` em consultas de lista

### ❌ Não Faça
```csharp
// ❌ Concatenação de SQL (SQL Injection)
var sql = $"SELECT * FROM USUARIO WHERE ID = {id}";

// ❌ SELECT * (retorna colunas desnecessárias)
var sql = "SELECT * FROM USUARIO";

// ❌ Bloqueio síncrono
var result = _session.Connection.Query<Usuario>(sql).ToList();

// ❌ Compartilhar conexão entre requisições
// Sempre use DbSession como Scoped
```

---

## Connection String — Oracle

### Padrão
```
User Id=usuario;Password=senha;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=servicename)));
```

### Com Pooling (Recomendado)
```
User Id=usuario;Password=senha;Data Source=...;Min Pool Size=2;Max Pool Size=20;Connection Lifetime=300;
```

### Configuração no appsettings.json
```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=...;Password=...;Data Source=...;Min Pool Size=2;Max Pool Size=20;Connection Lifetime=300;"
  }
}
```

---

## Mapeamento Oracle ← → C#

| Oracle | C# | Observação |
|--------|----|------------|
| `NUMBER(10)` | `int` | ID, contadores |
| `NUMBER(18,2)` | `decimal` | Valores monetários |
| `VARCHAR2(n)` | `string` | Texto curto |
| `CLOB` | `string` | Texto longo |
| `CHAR(1)` | `string` | Flags (S/N) |
| `DATE` | `DateTime` | Datas |
| `NUMBER(1)` | `bool` | Flags booleanas |

---

> **Consulte também:**
> - [skills/dapper-orm.md](../skills/dapper-orm.md) — Guia detalhado de Dapper
> - [skills/oracle-best-practices.md](../skills/oracle-best-practices.md) — Boas práticas Oracle