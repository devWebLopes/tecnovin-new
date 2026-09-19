# 🗄️ Skill: Dapper ORM com Oracle

## Sobre
Dapper é um micro-ORM desenvolvido pelo Stack Overflow. Extremamente rápido, extensível e com mapeamento direto entre queries SQL e objetos C#.

## Configuração Padrão

### DbSession (Gerenciamento de Conexão)

```csharp
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Empresa.Data;

public class DbSession : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public DbSession(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Connection =>
        _connection ??= new OracleConnection(_connectionString);

    public void Dispose() => _connection?.Dispose();
}
```

### Registro no Program.cs

```csharp
builder.Services.AddScoped<DbSession>(sp =>
    new DbSession(builder.Configuration.GetConnectionString("Oracle")));
```

## Padrão de Repositório

```csharp
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbSession _session;

    public UsuarioRepository(DbSession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        const string sql = "SELECT ID_USUARIO, NOME, LOGIN, ATIVO FROM USUARIO WHERE ATIVO = 'S'";
        return await _session.Connection.QueryAsync<Usuario>(sql);
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        const string sql = "SELECT ID_USUARIO, NOME, LOGIN, ATIVO FROM USUARIO WHERE ID_USUARIO = :Id";
        return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        const string sql = @"
            INSERT INTO USUARIO (NOME, LOGIN, SENHA, ATIVO)
            VALUES (:Nome, :Login, :Senha, :Ativo)
            RETURNING ID_USUARIO INTO :Id";

        var parameters = new DynamicParameters(usuario);
        parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }
}
```

## Regras Importantes

### Oracle vs SQL Server (cuidados)
- ❌ **Não use** `@p0`, `@nome` — Oracle usa `:p0`, `:nome`
- ❌ **Não use** `TOP` — Oracle usa `FETCH FIRST n ROWS ONLY` ou `ROWNUM`
- ❌ **Não use** `GETDATE()` — Oracle usa `SYSDATE` ou `CURRENT_TIMESTAMP`
- ✅ Use `RETURNING` para obter IDs gerados
- ✅ Trate strings vazias como NULL (Oracle faz isso automaticamente)

### Performance
- Sempre especifique colunas (nunca `SELECT *`)
- Use `WHERE` com índices apropriados
- Para listas grandes, implemente paginação com `OFFSET`/`FETCH NEXT`
- Evite `N+1 queries` — prefira JOINs

### Segurança
- Sempre use parâmetros (`new { ... }` ou `DynamicParameters`)
- Nunca concatene strings SQL
- Para buscas com `LIKE`, use `CONCAT('%', :termo, '%')` (Oracle)