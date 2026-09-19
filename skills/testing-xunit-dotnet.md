# 🧪 Skill: Testing Patterns — xUnit + .NET 9

## Sobre
Esta skill define o padrão completo de testes para o projeto GestaoNew. Stack: xUnit + Moq + FluentAssertions + AutoFixture. Cobre testes unitários para Services, testes de integração com Oracle e testes de segurança.

## Configuração do Projeto de Testes

### Empresa.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.2" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="AutoFixture" Version="4.18.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Empresa.Api\Empresa.Api.csproj" />
    <ProjectReference Include="..\Empresa.Data\Empresa.Data.csproj" />
  </ItemGroup>
</Project>
```

## Estrutura de Diretórios de Testes

```
Empresa.Tests/
├── Services/
│   ├── AuthServiceTests.cs
│   ├── UsuarioServiceTests.cs
│   ├── PerfilServiceTests.cs
│   ├── FinanceiroServiceTests.cs
│   ├── ComprasServiceTests.cs
│   └── VendasServiceTests.cs
├── Repositories/
│   ├── UsuarioRepositoryTests.cs
│   ├── AcessoRepositoryTests.cs
│   └── FinanceiroRepositoryTests.cs
├── Integration/
│   ├── AuthIntegrationTests.cs
│   └── DatabaseIntegrationTests.cs
├── Security/
│   └── AuthorizationTests.cs
└── Helpers/
    ├── TestDataFactory.cs
    └── MockDbConnection.cs
```

## Teste Unitário de Service (com Moq)

```csharp
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;
using Empresa.Api.Services;
using Empresa.Data.Repositories;
using Empresa.Data.Models;
using Empresa.Api.DTOs.Response;

namespace Empresa.Tests.Services;

public class UsuarioServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly UsuarioService _sut; // System Under Test

    public UsuarioServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior()); // Evita loops

        _repoMock = new Mock<IUsuarioRepository>();
        _sut = new UsuarioService(_repoMock.Object);
    }

    [Fact(DisplayName = "GetAllAsync deve retornar lista de usuários")]
    public async Task GetAllAsync_DeveRetornarListaDeUsuarios()
    {
        // Arrange
        var usuarios = _fixture.CreateMany<Usuario>(3).ToList();
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(usuarios);

        // Act
        var resultado = await _sut.GetAllAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(3);
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact(DisplayName = "GetByIdAsync com ID inexistente deve retornar null")]
    public async Task GetByIdAsync_IdInexistente_DeveRetornarNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _sut.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact(DisplayName = "CreateAsync deve chamar repositório com dados corretos")]
    public async Task CreateAsync_DeveChamarRepositorio()
    {
        // Arrange
        var request = _fixture.Create<UsuarioRequest>();
        var usuarioCriado = _fixture.Create<Usuario>();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Usuario>())).ReturnsAsync(usuarioCriado);

        // Act
        var resultado = await _sut.CreateAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        _repoMock.Verify(r => r.CreateAsync(It.Is<Usuario>(
            u => u.Nome == request.Nome && u.Login == request.Login)), Times.Once);
    }

    [Theory(DisplayName = "CreateAsync com dados inválidos deve lançar exceção")]
    [InlineData(null, "login", "senha123")]  // Nome nulo
    [InlineData("Nome", null, "senha123")]   // Login nulo
    [InlineData("Nome", "login", "")]        // Senha vazia
    [InlineData("Nome", "login", "12345")]   // Senha < 6 caracteres
    public async Task CreateAsync_DadosInvalidos_DeveLancarException(
        string? nome, string? login, string senha)
    {
        // Arrange
        var request = new UsuarioRequest(nome!, login!, senha, 1, "S");

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

## Teste de Segurança — Autenticação JWT

```csharp
namespace Empresa.Tests.Security;

public class AuthorizationTests
{
    private readonly HttpClient _client;

    public AuthorizationTests()
    {
        var factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Endpoint sem token deve retornar 401")]
    public async Task EndpointSemToken_DeveRetornar401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Endpoint com token inválido deve retornar 401")]
    public async Task EndpointComTokenInvalido_DeveRetornar401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token-invalido");

        // Act
        var response = await _client.GetAsync("/api/v1/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Login com credenciais corretas deve retornar JWT")]
    public async Task LoginCredenciaisCorretas_DeveRetornarJwt()
    {
        // Arrange
        var loginRequest = new
        {
            login = "admin",
            senha = "senha123"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(body);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }
}
```

## Teste de Integração com Oracle

```csharp
namespace Empresa.Tests.Integration;

// ⚠️ Estes testes exigem Oracle de desenvolvimento disponível
// Rodar apenas em ambiente com banco configurado
public class DatabaseIntegrationTests : IClassFixture<OracleFixture>
{
    private readonly OracleFixture _fixture;

    public DatabaseIntegrationTests(OracleFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Conexão Oracle deve estar ativa")]
    public async Task ConexaoOracle_DeveEstarAtiva()
    {
        // Act
        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        // Assert
        connection.State.Should().Be(ConnectionState.Open);
    }

    [Fact(DisplayName = "Consulta de usuário deve retornar dados")]
    public async Task ConsultaUsuario_DeveRetornarDados()
    {
        // Arrange
        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        // Act
        var usuarios = await connection.QueryAsync<Usuario>(
            "SELECT ID_USUARIO, NOME, LOGIN FROM USUARIO WHERE ROWNUM <= 5");

        // Assert
        usuarios.Should().NotBeNull();
    }
}

// Helper — OracleFixture
public class OracleFixture : IDisposable
{
    private readonly string _connectionString;

    public OracleFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")
            ?? "User Id=teste;Password=teste;Data Source=localhost:1521/XE";
    }

    public IDbConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }

    public void Dispose() { }
}
```

## Teste de Repositório com Mock de IDbConnection

```csharp
namespace Empresa.Tests.Repositories;

public class UsuarioRepositoryTests
{
    private readonly Mock<DbSession> _dbSessionMock;
    private readonly Mock<IDbConnection> _connectionMock;
    private readonly UsuarioRepository _sut;

    public UsuarioRepositoryTests()
    {
        _connectionMock = new Mock<IDbConnection>();
        _dbSessionMock = new Mock<DbSession>("dummy");
        _dbSessionMock.Setup(s => s.Connection).Returns(_connectionMock.Object);

        _sut = new UsuarioRepository(_dbSessionMock.Object);
    }

    [Fact(DisplayName = "GetAllAsync deve executar query correta")]
    public async Task GetAllAsync_DeveExecutarQueryCorreta()
    {
        // Arrange — como Dapper é extension method, testamos via integração
        // Para testes unitários puros de repositório, use DbSession real + Oracle de dev
        // Ou refatore para abstrair o Dapper (não recomendado — over-engineering)
    }
}
```

> ⚠️ **Estratégia recomendada:** Testar Services com Moq (unitários) e Repositories com Oracle de dev (integração). Mockar IDbConnection é frágil e de baixo valor.

## Teste de Validação de Senha (BCrypt)

```csharp
namespace Empresa.Tests.Services;

public class SenhaSegurancaTests
{
    [Fact(DisplayName = "Senha com hash deve ser verificada corretamente")]
    public void SenhaComHash_DeveSerVerificadaCorretamente()
    {
        // Arrange
        var senha = "MinhaSenha@123";
        var hash = SenhaHelper.HashSenha(senha);

        // Act
        var valida = SenhaHelper.VerificarSenha(senha, hash);
        var invalida = SenhaHelper.VerificarSenha("senhaErrada", hash);

        // Assert
        hash.Should().NotBe(senha);               // Hash ≠ texto plano
        valida.Should().BeTrue();                  // Senha correta verifica
        invalida.Should().BeFalse();               // Senha errada não verifica
    }

    [Fact(DisplayName = "Hash deve ser diferente para mesma senha (salt aleatório)")]
    public void Hash_DeveSerDiferenteParaMesmaSenha()
    {
        // Arrange
        var senha = "Teste@123";

        // Act
        var hash1 = SenhaHelper.HashSenha(senha);
        var hash2 = SenhaHelper.HashSenha(senha);

        // Assert
        hash1.Should().NotBe(hash2); // Salt aleatório → hashes diferentes
    }
}
```

## Regras Obrigatórias

1. **SEMPRE** usar o padrão AAA: Arrange → Act → Assert
2. **SEMPRE** usar `DisplayName` nos `[Fact]` e `[Theory]` — descrições em português
3. **SEMPRE** usar `FluentAssertions` para asserts legíveis — nunca `Assert.Equal()` do xUnit
4. **SEMPRE** nomear testes com: `Metodo_Cenario_ResultadoEsperado`
5. **SEMPRE** testar cenários felizes E infelizes (null, exceção, dados inválidos)
6. **SEMPRE** usar `Theory` com `[InlineData]` para múltiplos casos de borda
7. **NUNCA** testar implementação interna (private methods) — apenas comportamento público
8. **SEMPRE** verificar `Times.Once` nos mocks para garantir que o repositório foi chamado
9. **COBERTURA MÍNIMA:** ≥ 60% (medida via `coverlet`)
10. **SEMPRE** isolar testes — nunca depender de estado compartilhado entre testes

## Comandos

```bash
# Rodar todos os testes
dotnet test

# Rodar com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Rodar testes de um módulo específico
dotnet test --filter "FullyQualifiedName~UsuarioServiceTests"

# Rodar em watch mode (desenvolvimento)
dotnet watch test --project Empresa.Tests