# 🧪 Testes — GestãoNew

## Visão Geral

Este documento define as diretrizes para testes no projeto GestãoNew. O projeto atualmente **não possui** projetos de teste criados, mas eles devem ser adicionados seguindo os padrões abaixo.

---

## Estrutura de Testes

### Organização

Os testes devem ficar em projetos separados na solution:

```
GestaoNew/
├── Empresa.Api/                          # Projeto principal
├── Empresa.Data/                         # Projeto principal
├── Empresa.Util/                         # Projeto principal
├── Empresa.Worker/                       # Projeto principal
│
├── tests/
│   ├── Empresa.Api.Tests/               # Testes de API
│   ├── Empresa.Data.Tests/              # Testes de dados
│   ├── Empresa.Util.Tests/              # Testes de utilitários
│   └── Empresa.Worker.Tests/            # Testes de Worker
```

### Projeto de Teste (csproj)

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
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Empresa.Data\Empresa.Data.csproj" />
  </ItemGroup>

</Project>
```

---

## Framework de Testes

| Ferramenta | Versão | Finalidade |
|-----------|--------|------------|
| xUnit | 2.9+ | Framework de testes |
| Moq | 4.20+ | Mocking de dependências |
| FluentAssertions | 6.12+ | Assertions legíveis |
| Microsoft.NET.Test.Sdk | 17.x | SDK de testes |
| coverlet | 6.x | Cobertura de código |

---

## Padrões de Teste por Camada

### 1. Testes de Service (Empresa.Api.Tests)

```csharp
namespace Empresa.Api.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _service = new UsuarioService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUsuarioExists_ReturnsUsuarioResponse()
    {
        // Arrange
        var usuario = new Usuario
        {
            IDUsuario = 1,
            Nome = "João Silva",
            Login = "joao",
            Ativo = "S"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("João Silva");
        result.Login.Should().Be("joao");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUsuarioDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenLoginAlreadyExists_ThrowsBusinessException()
    {
        // Arrange
        var request = new UsuarioRequest
        {
            Nome = "João",
            Login = "joao",
            Senha = "123456"
        };

        _repositoryMock
            .Setup(r => r.LoginExistsAsync("joao"))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Já existe um usuário cadastrado com este login");
    }
}
```

### 2. Testes de Utilitários (Empresa.Util.Tests)

```csharp
namespace Empresa.Util.Tests;

public class StringUtilsTests
{
    [Theory]
    [InlineData("nome.com.valor", "nome_com_valor")]
    [InlineData("nome com espaços", "nome_com_espaços")]
    [InlineData("100% completo", "100__completo")]
    [InlineData("a+b-c", "a_b_c")]
    public void TrocaCaracteres_ReplacesSpecialCharacters(string input, string expected)
    {
        // Act
        var result = StringUtils.TrocaCaracteres(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TrocaCaracteres_WhenInputIsEmpty_ReturnsEmpty()
    {
        // Act
        var result = StringUtils.TrocaCaracteres(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TrocaCaracteres_WhenInputIsNull_ThrowsException()
    {
        // Act
        var act = () => StringUtils.TrocaCaracteres(null!);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
```

### 3. Testes de Repositório (Empresa.Data.Tests)

Para testes de integração com banco real:

```csharp
namespace Empresa.Data.Tests.Repositories;

public class UsuarioRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UsuarioRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_InsertsUsuarioAndReturnsId()
    {
        // Arrange
        var repository = new UsuarioRepository(_fixture.DbSession);
        var usuario = new Usuario
        {
            Nome = "Teste",
            Login = $"teste_{Guid.NewGuid():N}",
            Senha = "hash123",
            Ativo = "S"
        };

        // Act
        var id = await repository.CreateAsync(usuario);

        // Assert
        id.Should().BeGreaterThan(0);

        // Cleanup
        await repository.DeleteAsync(id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUsuarioExists_ReturnsUsuario()
    {
        // Arrange
        var repository = new UsuarioRepository(_fixture.DbSession);

        // First create a test user
        var usuario = new Usuario
        {
            Nome = "Teste Get",
            Login = $"teste_get_{Guid.NewGuid():N}",
            Senha = "hash123",
            Ativo = "S"
        };
        var id = await repository.CreateAsync(usuario);

        // Act
        var result = await repository.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.IDUsuario.Should().Be(id);
        result.Nome.Should().Be("Teste Get");

        // Cleanup
        await repository.DeleteAsync(id);
    }
}
```

### DatabaseFixture para Testes de Integração

```csharp
namespace Empresa.Data.Tests;

public class DatabaseFixture : IDisposable
{
    public DbSession DbSession { get; }

    public DatabaseFixture()
    {
        var connectionString = "User Id=test;Password=test;Data Source=...";
        DbSession = new DbSession(connectionString);
    }

    public void Dispose()
    {
        DbSession.Dispose();
    }
}
```

---

## Nomenclatura de Testes

### Padrão: `[UnidadeTestada]_[Cenario]_[ResultadoEsperado]`

```csharp
// ✅ Correto
public async Task GetByIdAsync_WhenUsuarioExists_ReturnsUsuarioResponse()
public async Task CreateAsync_WhenLoginAlreadyExists_ThrowsBusinessException()
public async Task TrocaCaracteres_WhenInputHasDots_ReplacesWithUnderscore()

// ❌ Incorreto
public async Task Test1()
public async Task GetUsuario()
```

### Organização das Classes de Teste

```
Empresa.Api.Tests/
├── Services/
│   ├── UsuarioServiceTests.cs
│   └── PaginaServiceTests.cs
├── Endpoints/
│   ├── UsuarioEndpointsTests.cs
│   └── ...
└── Middleware/
    └── ExceptionMiddlewareTests.cs
```

---

## Estrutura de um Teste (AAA)

Sempre seguir o padrão **Arrange-Act-Assert**:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - preparação dos dados e mocks
    var mockRepo = new Mock<IUsuarioRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
    var service = new UsuarioService(mockRepo.Object);

    // Act - execução do método testado
    var result = await service.GetByIdAsync(1);

    // Assert - verificação do resultado
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
}
```

---

## Tipos de Teste

### Testes Unitários
- Testam uma única classe/método
- Dependências externas são mockadas (Moq)
- Rápidos (ms) e sem dependência de infraestrutura
- **Obrigatórios** para Services e Utilitários

### Testes de Integração
- Testam a interação com banco Oracle real
- Usam `DatabaseFixture` para setup/teardown
- Mais lentos, executados separadamente
- **Recomendados** para Repositórios

### Testes de Endpoint
- Testam a API HTTP completa
- Usam `WebApplicationFactory` para subir a aplicação em memória
- **Opcionais** (prioridade menor)

---

## Padrões com Moq

```csharp
// Setup básico
mock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

// Qualquer valor
mock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(usuario);

// Validação de chamada
mock.Verify(r => r.GetByIdAsync(1), Times.Once);
mock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);

// Callback
mock.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
    .Callback<Usuario>(u => u.IDUsuario = 1)
    .ReturnsAsync(1);
```

---

## Cobertura de Código

### Meta
- **Mínimo 70%** de cobertura em Services
- **Obrigatório** cobrir cenários de erro (exceções)
- **Mínimo 80%** em Utilitários

### Executando Cobertura

```bash
# Com coverlet
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório HTML (precisa do reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

---

## Comandos para Executar Testes

```bash
# Executar todos os testes
dotnet test

# Executar com verbose
dotnet test --verbosity normal

# Executar testes de um projeto específico
dotnet test tests/Empresa.Api.Tests/Empresa.Api.Tests.csproj

# Executar com filtro
dotnet test --filter "FullyQualifiedName~UsuarioService"

# Executar e gerar cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./test-results"
```

---

## Boas Práticas

### ✅ Faça
- Teste o comportamento, não a implementação
- Um teste = uma asserção lógica
- Use `[Theory]` e `[InlineData]` para múltiplos cenários
- Teste cenários de sucesso e erro
- Mantenha testes independentes entre si
- Use nomes descritivos em português nos `ErrorMessage`

### ❌ Não Faça
```csharp
// ❌ Teste frágil (acoplado à implementação)
mock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));

// ❌ Múltiplas asserções não relacionadas
result.Id.Should().Be(1);
result.Nome.Should().Be("João");
result.Login.Should().Be("joao");
// OK se fazem parte do mesmo comportamento

// ❌ Teste que depende de outro teste
[Fact]
public async Task TestCreateFirstThenGet() // dependência de estado
{
    var id = await service.CreateAsync(request);
    var result = await service.GetByIdAsync(id);
    result.Should().NotBeNull();
}
```

---

> **Nota:** Os projetos de teste ainda não foram criados. Siga este guia ao implementá-los.