# 🔒 Segurança — GestãoNew

## Índice

1. [Senhas e Hashing](#1-senhas-e-hashing)
2. [Proteção contra SQL Injection](#2-proteção-contra-sql-injection)
3. [Configurações Sensíveis](#3-configurações-sensíveis)
4. [Serviço de E-mail](#4-serviço-de-e-mail)
5. [Boas Práticas no Oracle](#5-boas-práticas-no-oracle)
6. [Checklist de Segurança](#6-checklist-de-segurança)

---

## 1. Senhas e Hashing

### Regra Obrigatória
**Jamais armazene senhas em texto plano.** O campo `Senha` na tabela `USUARIO` deve sempre conter o hash da senha.

### Algoritmo Recomendado: BCrypt

```csharp
// Instalar: dotnet add package BCrypt.Net-Next

using BCrypt.Net;

/// <summary>
/// Utilitário para hash e verificação de senhas usando BCrypt
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12; // 2^12 rounds (recomendado: 10-12)

    /// <summary>
    /// Gera o hash da senha com salt automático
    /// </summary>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    /// <summary>
    /// Verifica se a senha corresponde ao hash armazenado
    /// </summary>
    public static bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### Alternativa: PBKDF2 (nativo do .NET)

```csharp
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16;    // 128 bits
    private const int KeySize = 32;     // 256 bits
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);

        var salt = algorithm.Salt;
        var key = algorithm.GetBytes(KeySize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hash)
    {
        var parts = hash.Split('.');
        var salt = Convert.FromBase64String(parts[0]);
        var key = Convert.FromBase64String(parts[1]);

        using var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var keyToCheck = algorithm.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
    }
}
```

### Uso no Service

```csharp
public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request)
{
    var usuario = new Usuario
    {
        Nome = request.Nome,
        Login = request.Login,
        Senha = PasswordHasher.Hash(request.Senha), // ✅ Hash antes de salvar
        Ativo = "S"
    };

    var id = await _repository.CreateAsync(usuario);
    return MapToResponse(usuario);
}

public async Task<bool> ValidatePasswordAsync(string login, string password)
{
    var usuario = await _repository.GetByLoginAsync(login);
    
    if (usuario is null)
        return false;

    return PasswordHasher.Verify(password, usuario.Senha);
}
```

---

## 2. Proteção contra SQL Injection

### Regra Absoluta
**Nunca concatene strings para construir SQL.** Use sempre parâmetros vinculados (bind variables).

```csharp
// ✅ SEGURO - Usa parâmetros
const string sql = "SELECT * FROM USUARIO WHERE LOGIN = :Login AND SENHA = :Senha";
var usuario = await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
    sql, new { Login = login, Senha = senhaHash });

// ❌ INSEGURO - Concatenação
var sql = $"SELECT * FROM USUARIO WHERE LOGIN = '{login}'"; // SQL Injection!
```

### Como o Dapper Protege
- Dapper sempre usa `DbCommand` com parâmetros
- Utiliza `OracleParameter` internamente (bind variables)
- Os valores são escapados pelo driver Oracle, não pelo C#

---

## 3. Configurações Sensíveis

### Regras
- **Senhas de banco** e **credenciais SMTP** nunca no código fonte
- Use **User Secrets** em desenvolvimento
- Use **Environment Variables** ou **Azure Key Vault** em produção

### User Secrets (Desenvolvimento)

```bash
# Inicializar (uma vez por projeto)
dotnet user-secrets init --project Empresa.Api

# Configurar valores
dotnet user-secrets set "ConnectionStrings:Oracle" "User Id=...;Password=...;"
dotnet user-secrets set "Email:smtpSenha" "sua-senha"
```

### Acessando Configurações

```csharp
// A ordem de precedência é:
// 1. Environment Variables
// 2. User Secrets (desenvolvimento)
// 3. appsettings.Development.json
// 4. appsettings.json

var connectionString = builder.Configuration.GetConnectionString("Oracle");
var smtpSenha = builder.Configuration["Email:smtpSenha"];
```

### appsettings.json (produção - sem senhas)
```json
{
  "ConnectionStrings": {
    "Oracle": ""  // Em branco - configurado via environment
  },
  "Email": {
    "smtpServidor": "smtp.empresa.com",
    "smtpPort": 587,
    "smtpEmail": "naoresponda@empresa.com",
    "smtpNome": "GestãoNew",
    "smtpSenha": ""  // Em branco - configurado via environment
  }
}
```

---

## 4. Serviço de E-mail

### Configuração Segura

O serviço de e-mail (`Empresa.Util.Email`) lê as configurações do `IConfiguration`. As senhas nunca devem estar no código.

```csharp
// Empresa.Util/Email.cs
private string GetAppSettings(string nome)
{
    return _configuration[$"Email:{nome}"] 
           ?? _configuration.GetSection("Email")[nome] 
           ?? string.Empty;
}
```

### Config via Environment Variables (Produção)

```bash
# Windows
setx Email__smtpSenha "senha-segura"

# Linux / Docker
export Email__smtpSenha="senha-segura"
```

---

## 5. Boas Práticas no Oracle

### Privilégios Mínimos
- Conceda apenas os privilégios necessários para a aplicação
- Use uma conta de serviço dedicada (não o schema owner)

```sql
-- ❌ Muito permissivo
GRANT ALL PRIVILEGES TO app_user;

-- ✅ Apenas o necessário
GRANT CONNECT TO app_user;
GRANT CREATE SESSION TO app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON USUARIO TO app_user;
```

### Bind Variables
O Oracle utiliza bind variables automaticamente quando você usa Dapper com parâmetros, o que:
- Previne SQL Injection
- Melhora performance (reuso de plano de execução)
- Evita problemas com caracteres especiais

```sql
-- Oracle otimiza automaticamente quando usa bind variables
SELECT * FROM USUARIO WHERE LOGIN = :Login
```

### Consultas Seguras

```csharp
// ✅ Seguro - parâmetros vinculados
const string sql = @"
    SELECT * FROM USUARIO 
    WHERE UPPER(NOME) LIKE CONCAT('%', CONCAT(UPPER(:Termo), '%'))";
var usuarios = await _session.Connection.QueryAsync<Usuario>(sql, new { Termo = busca });

// ✅ Seguro - busca por ID
const string sql = "SELECT * FROM USUARIO WHERE ID_USUARIO = :Id";
var usuario = await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
```

---

## 6. Checklist de Segurança

### ✅ Obrigatório
- [ ] Senhas armazenadas com BCrypt ou PBKDF2 (nunca texto plano)
- [ ] Todas as queries SQL usam parâmetros (bind variables)
- [ ] Conexão com banco usa pooling (Min Pool Size, Max Pool Size)
- [ ] Configurações sensíveis em User Secrets ou Environment Variables
- [ ] Serviço de e-mail usa configurações externas (não hardcoded)

### ✅ Recomendado
- [ ] Autenticação via JWT nos endpoints (quando implementado)
- [ ] Rate limiting em endpoints de login
- [ ] HTTPS obrigatório em produção
- [ ] CORS configurado corretamente
- [ ] Logging de tentativas de acesso inválidas
- [ ] Validação de entrada em todos os endpoints

---

> **Consulte também:** [data-layer.md](data-layer.md) para boas práticas de queries seguras.