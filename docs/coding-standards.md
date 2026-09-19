# ✍️ Padrões de Código — GestãoNew

## Índice

1. [Linguagem e Estilo](#1-linguagem-e-estilo)
2. [Formatação](#2-formatação)
3. [Estrutura de Arquivos](#3-estrutura-de-arquivos)
4. [Async/Await](#4-asyncawait)
5. [Tratamento de Erros](#5-tratamento-de-erros)
6. [Injeção de Dependência](#6-injeção-de-dependência)
7. [Uso de `var` vs Tipos Explícitos](#7-uso-de-var-vs-tipos-explícitos)
8. [Documentação de Código](#8-documentação-de-código)
9. [Disposição de Recursos](#9-disposição-de-recursos)
10. [Regras Específicas do Projeto](#10-regras-específicas-do-projeto)

---

## 1. Linguagem e Estilo

### Idiomas
- **Código (classes, métodos, propriedades):** Inglês
- **Comentários e documentação XML:** Português
- **Strings de usuário/mensagens:** Conforme necessidade do negócio

```csharp
/// <summary>
/// Repositório de usuários com acesso ao banco Oracle via Dapper
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        const string sql = "SELECT ID_USUARIO, NOME, LOGIN FROM USUARIO WHERE ATIVO = 'S'";
        return await _session.Connection.QueryAsync<Usuario>(sql);
    }
}
```

### Nullable Reference Types
- **Habilitado** em todos os projetos (`<Nullable>enable</Nullable>`)
- Use `?` para tipos que podem ser null: `string?`, `Usuario?`
- Use `string.Empty` em vez de `""` para evitar null warnings

```csharp
// ✅ Correto
public string Nome { get; set; } = string.Empty;
public int? IDPaginaPai { get; set; }

// ❌ Incorreto
public string Nome { get; set; } = "";
```

### ImplicitUsings
- **Habilitado** em todos os projetos (`<ImplicitUsings>enable</ImplicitUsings>`)
- Não adicione `using System;`, `using System.Threading.Tasks;` etc. manualmente — já estão implícitos
- Adicione `using` apenas para namespaces externos ou do próprio projeto

---

## 2. Formatação

### Indentação
- **4 espaços** (não use tabs)
- Configure o Visual Studio para inserir espaços ao pressionar Tab

### Chaves
- **Sempre na mesma linha** (estilo K&R / Allman para .NET)
- Use `{ }` mesmo para blocos de uma linha (consistência)

```csharp
// ✅ Correto
if (usuario is null) {
    return Results.NotFound();
}

// ✅ Também aceito (preferido no projeto)
if (usuario is null)
    return Results.NotFound();

// ❌ Incorreto - sem chaves
if (usuario is null) return Results.NotFound();
```

### Linhas em Branco
- 1 linha entre métodos
- 2 linhas entre classes/grupos de código
- 1 linha entre blocos lógicos dentro de um método

### Comprimento de Linha
- Preferencialmente até **120 caracteres**
- Quebre linhas longas após operadores (`.` , `=>` , `,`)

---

## 3. Estrutura de Arquivos

### Ordem dos Membros na Classe
```csharp
public class UsuarioService : IUsuarioService
{
    // 1. Constantes
    private const int DefaultPageSize = 20;

    // 2. Campos privados
    private readonly IUsuarioRepository _repository;

    // 3. Construtor
    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    // 4. Propriedades públicas
    public int CurrentCount { get; set; }

    // 5. Métodos públicos (em ordem alfabética)
    public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request) { ... }
    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync() { ... }

    // 6. Métodos privados
    private UsuarioResponse MapToResponse(Usuario usuario) { ... }
}
```

### Ordem dos Usings
1. System.*
2. Third-party (Dapper, Oracle)
3. Microsoft.*
4. Empresa.* (próprio projeto)

---

## 4. Async/Await

### Regras Obrigatórias
- ✅ Toda operação de I/O (banco, arquivo, rede) deve ser `async Task`
- ✅ Async all the way: não misture sync com async
- ✅ Use `await` em vez de `.Result` ou `.Wait()`
- ✅ Métodos async devem ter sufixo `Async`
- ✅ Use `ValueTask` apenas em hot paths (alta performance)

### Proibições
```csharp
// ❌ PROIBIDO
var result = _repository.GetAllAsync().Result;
_repository.GetAllAsync().Wait();
Task.WaitAll(task1, task2);

// ✅ CORRETO
var result = await _repository.GetAllAsync();
await Task.WhenAll(task1, task2);
```

---

## 5. Tratamento de Erros

### Regras
- Use `IResult` padronizado nos endpoints: `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`
- Use middleware global para exceções não tratadas
- Não use `try-catch` para fluxo normal (apenas para exceções reais)
- Exceções são para situações **excepcionais**, não para controle de fluxo

```csharp
// ✅ Correto
app.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
{
    var usuario = await service.GetByIdAsync(id);
    return usuario is null ? Results.NotFound() : Results.Ok(usuario);
});

// ❌ Incorreto
app.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
{
    try {
        var usuario = await service.GetByIdAsync(id);
        return Results.Ok(usuario);
    } catch (Exception ex) {
        return Results.Problem(ex.Message);
    }
});
```

---

## 6. Injeção de Dependência

### Regras
- ✅ Use apenas `Microsoft.Extensions.DependencyInjection`
- ✅ Registre serviços com o ciclo de vida apropriado:
  - **Scoped:** DbSession, repositórios (1 por requisição)
  - **Transient:** Serviços leves e stateless
  - **Singleton:** Logger, configurações
- ✅ Injete dependências via construtor
- ❌ Proibido: Service Locator (`IServiceProvider.GetService`)

```csharp
// ✅ Correto
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbSession _session;

    public UsuarioRepository(DbSession session)
    {
        _session = session;
    }
}
```

---

## 7. Uso de `var` vs Tipos Explícitos

### Use `var` quando:
- O tipo é óbvio pelo lado direito: `var list = new List<int>();`
- O tipo é complexo (genéricos aninhados): `var result = await GetDataAsync();`

### Use tipo explícito quando:
- O tipo não é óbvio: `int count = GetCount();`
- O tipo difere do retorno do método (polimorfismo)

```csharp
// ✅ var (óbvio)
var usuario = new Usuario();
var usuarios = await repository.GetAllAsync();

// ✅ Explícito (precisa ficar claro)
IEnumerable<Usuario> usuarios = repository.GetAll(); // Lazy evaluation importante
int total = CalculateTotal();
```

---

## 8. Documentação de Código

### Comentários XML
- Use `///` em todas as classes e interfaces públicas
- Descreva o **propósito**, não o **como**

```csharp
/// <summary>
/// Serviço para gerenciamento de usuários do sistema
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Retorna todos os usuários ativos
    /// </summary>
    Task<IEnumerable<UsuarioResponse>> GetAllAsync();

    /// <summary>
    /// Busca um usuário pelo seu ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Dados do usuário ou null se não encontrado</returns>
    Task<UsuarioResponse?> GetByIdAsync(int id);
}
```

### Comentários Inline
- Use `//` para explicar **por que** o código faz algo, não **o que** faz
- Código deve ser auto-documentado — se você precisa explicar o que faz, talvez o código precise ser refatorado

```csharp
// ✅ Útil: explica o motivo de uma decisão
// Usa cache de 5 min porque este dado muda raramente
const int cacheDuration = 300;

// ❌ Inútil: explica o óbvio
// Soma dois números
return a + b;
```

---

## 9. Disposição de Recursos

### Regras
- ✅ Use `using` ou `await using` para recursos não gerenciados
- ✅ DbSession implementa `IDisposable`
- ✅ Conexões devem ser descartadas após uso

```csharp
// ✅ Correto
using var connection = new OracleConnection(connectionString);

// await using para IAsyncDisposable
await using var transaction = await connection.BeginTransactionAsync();
```

---

## 10. Regras Específicas do Projeto

### SQL em Repositórios
- Use `const string sql` para queries estáticas
- Sempre use parâmetros com `:` prefixo Oracle
- Proibido: concatenação de strings SQL

```csharp
// ✅ Correto
const string sql = "SELECT * FROM USUARIO WHERE ID_USUARIO = :Id";
return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });

// ❌ Incorreto
var sql = $"SELECT * FROM USUARIO WHERE ID_USUARIO = {id}";
```

### Constantes de Formatação
Use as constantes definidas em `Empresa.Util.Dados`:

```csharp
using Empresa.Util;

Console.WriteLine(Dados.FormatoInteiro, 1234);      // "1,234"
Console.WriteLine(Dados.FormatoDecimal, 1234.56);    // "1,234.56"
Console.WriteLine(Dados.FormatoPercentual, 12.5);    // "12.5"
```

---

## ⚙️ Configuração do EditorConfig

O projeto segue as configurações padrão do .NET. Recomenda-se usar um arquivo `.editorconfig` na raiz com:

```ini
root = true

[*]
indent_style = space
indent_size = 4
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.cs]
dotnet_sort_system_directives_first = true
csharp_using_directive_placement = outside_namespace
csharp_prefer_braces = true
csharp_style_var_for_built_in_types = true
csharp_style_var_when_type_is_apparent = true
```

---

> **Dica:** Configure o Visual Studio para aplicar formatação automática ao salvar (`Tools > Options > Text Editor > C# > Advanced > Format on save`).