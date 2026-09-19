using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Empresa.Data;

/// <summary>
/// Gerencia a conexão com Oracle via Dapper.
/// Registrado como Scoped - uma conexão por requisição.
/// </summary>
public class DbSession : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public DbSession(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Connection string 'Oracle' não encontrada em appsettings.json");
    }

    /// <summary>
    /// Construtor para testes — permite injetar uma conexão fake sem acessar Oracle.
    /// </summary>
    public DbSession(IDbConnection connection)
    {
        _connection = connection;
        _connectionString = string.Empty;
    }

    /// <summary>
    /// Retorna a conexão ativa (cria se não existir)
    /// </summary>
    public IDbConnection Connection =>
        _connection ??= new OracleConnection(_connectionString);

    /// <summary>
    /// Cria uma nova conexão (para uso em health checks ou operações isoladas)
    /// </summary>
    public IDbConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}