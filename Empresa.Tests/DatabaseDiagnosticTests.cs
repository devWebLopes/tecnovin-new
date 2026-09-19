using Empresa.Data;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using Dapper;

namespace Empresa.Tests;

public class DatabaseDiagnosticTests
{
    [Fact]
    public async Task TestOracleConnectionAndUserQuery()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Oracle"] = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.4)(PORT=1526)))(CONNECT_DATA=(SERVICE_NAME=TECNOVIN)));User ID=treisbi_teste2;Password=treisbi_teste2;"
            })
            .Build();

        var session = new DbSession(config);
        using var conn = session.CreateConnection();
        try
        {
            conn.Open();
            var result = await conn.QueryFirstOrDefaultAsync<string>("SELECT 'OK' FROM DUAL");
            Assert.Equal("OK", result);

            var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM USUARIO WHERE LOGIN = 'treis'");
            if (usuario != null)
            {
                Console.WriteLine($"Usuario encontrado: {usuario.LOGIN}, Senha: {usuario.SENHA}");
            }
            else
            {
                Console.WriteLine("Usuario treis não encontrado");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no teste de banco: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}
