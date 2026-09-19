# 📛 Convenções de Nomenclatura — GestãoNew

## Índice

1. [C# (Classes, Métodos, Propriedades)](#1-c-classes-métodos-propriedades)
2. [Arquivos e Diretórios](#2-arquivos-e-diretórios)
3. [Banco de Dados Oracle](#3-banco-de-dados-oracle)
4. [Namespaces](#4-namespaces)
5. [APIs e Endpoints](#5-apis-e-endpoints)
6. [DTOs](#6-dtos)
7. [Interfaces](#7-interfaces)

---

## 1. C# (Classes, Métodos, Propriedades)

### Classes e Registros
| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Classe pública | PascalCase | `UsuarioService`, `EmailService` |
| Classe estática | PascalCase | `StringUtils`, `Dados` |
| Record | PascalCase | `WeatherForecast`, `UsuarioRequest` |
| Classe de resposta | PascalCase + Sujeito | `ExceptionMiddleware` |

### Métodos
| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Método público | PascalCase | `GetAllAsync()`, `CreateAsync()` |
| Método privado | PascalCase | `GetAppSettings()` |
| Método async | PascalCase + Async suffix | `GetAllAsync()`, `SendAsync()` |
| Extension method | PascalCase | `MapUsuarioEndpoints()` |

### Propriedades e Campos
| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Propriedade pública | PascalCase | `IDUsuario`, `Nome`, `Ativo` |
| Campo privado (readonly) | `_camelCase` | `_configuration`, `_connection` |
| Campo privado (variável) | `camelCase` | `maxstring`, `oEmail` |
| Constante pública | PascalCase | `FormatoInteiro`, `FormatoDecimal` |
| Constante privada | PascalCase | `DefaultPageSize` |

### Parâmetros
| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Parâmetro de método | camelCase | `(int id)`, `(string nome)` |
| Parâmetro de construtor | camelCase | `(ILogger<Worker> logger)` |

### Variáveis Locais
| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Variável local | camelCase | `var summaries = ...` |
| Variável de descarte | `_` | `_ = await ...` |
| Variável de tupla | camelCase | `var (nome, idade) = ...` |

---

## 2. Arquivos e Diretórios

### Arquivos
| Tipo | Convenção | Exemplo |
|------|-----------|---------|
| Arquivo C# | PascalCase.cs | `UsuarioService.cs`, `Program.cs` |
| Arquivo de projeto | PascalCase.csproj | `Empresa.Api.csproj` |
| Arquivo de configuração | camelCase.json | `appsettings.json`, `launchSettings.json` |
| Arquivo de console | kebab-case.http | `Empresa.Api.http` |
| Arquivo de teste | PascalCase + Tests.cs | `UsuarioServiceTests.cs` |

### Diretórios
| Tipo | Convenção | Exemplo |
|------|-----------|---------|
| Diretório de projeto | PascalCase | `Empresa.Api/`, `Empresa.Data/` |
| Subdiretório de código | PascalCase | `Endpoints/`, `Services/`, `Models/` |
| Diretório de documentação | lowercase | `docs/`, `skills/`, `agents/` |

---

## 3. Banco de Dados Oracle

### Tabelas
- **Formato:** MAIÚSCULO com underscores
- **Exemplos:** `USUARIO`, `PAGINA`, `PERFIL`
- **Singular:** `USUARIO` (não `USUARIOS`)
- **Prefixo opcional por módulo:** `SEG_USUARIO`

### Colunas
- **Formato:** MAIÚSCULO com underscores
- **Chave primária:** `ID_` + nome da tabela → `ID_USUARIO`, `ID_PAGINA`
- **Chave estrangeira:** `ID_` + nome da tabela referenciada → `ID_PERFIL`, `ID_PAGINA_PAI`
- **Flags:** `ATIVO`, `BLOQUEADO`, `ATUALIZA_SENHA`
- **Datas:** prefixo `DT_` → `DT_CADASTRO`, `DT_ALTERACAO`, `DT_ACESSO`

### Constraints
| Tipo | Padrão | Exemplo |
|------|--------|---------|
| PK | `PK_` + tabela | `PK_USUARIO` |
| FK | `FK_` + tabela_origem + `_` + tabela_destino | `FK_USUARIO_PERFIL` |
| UK | `UK_` + tabela + `_` + coluna | `UK_USUARIO_LOGIN` |
| IX | `IX_` + tabela + `_` + coluna | `IX_USUARIO_NOME` |

---

## 4. Namespaces

### Padrão Geral
```
Empresa.<Camada>.<Subdominio>
```

### Exemplos por Camada
| Camada | Padrão | Exemplo |
|--------|--------|---------|
| API | `Empresa.Api.<Subdir>` | `Empresa.Api.Endpoints` |
| Data | `Empresa.Data.<Subdir>` | `Empresa.Data.Repositories` |
| Util | `Empresa.Util` | `Empresa.Util` (sem subdiretório) |
| Worker | `Empresa.Worker` | `Empresa.Worker` |

### File-Scoped Namespaces
Sempre usar **file-scoped namespaces** (C# 10+):
```csharp
// ✅ Correto
namespace Empresa.Data.Repositories;

// ❌ Incorreto
namespace Empresa.Data.Repositories
{
    ...
}
```

---

## 5. APIs e Endpoints

### Rotas
| Método | Padrão | Exemplo |
|--------|--------|---------|
| GET lista | `/api/<dominio>` | `GET /api/usuarios` |
| GET único | `/api/<dominio>/{id}` | `GET /api/usuarios/5` |
| POST | `/api/<dominio>` | `POST /api/usuarios` |
| PUT | `/api/<dominio>/{id}` | `PUT /api/usuarios/5` |
| DELETE | `/api/<dominio>/{id}` | `DELETE /api/usuarios/5` |

### Plural vs Singular
- **Sempre plural** nos endpoints: `/api/usuarios`, `/api/paginas`
- **Singular** no nome da classe: `Usuario`, `Pagina`

---

## 6. DTOs

### Request DTOs
| Padrão | Exemplo |
|--------|---------|
| `<Entidade>Request` | `UsuarioRequest`, `PaginaRequest` |
| `<Acao><Entidade>Request` | `CreateUsuarioRequest`, `UpdateUsuarioRequest` |

### Response DTOs
| Padrão | Exemplo |
|--------|---------|
| `<Entidade>Response` | `UsuarioResponse`, `PaginaResponse` |
| `<Entidade>ListResponse` | `UsuarioListResponse` (para listas paginadas) |

---

## 7. Interfaces

### Padrão
- Prefixo `I` maiúsculo
- Sempre PascalCase
- Nome descritivo + sufixo `Service`, `Repository`, `Provider`

| Tipo | Exemplo |
|------|---------|
| Service | `IUsuarioService` |
| Repository | `IUsuarioRepository` |
| Provider | `IEmailProvider` |

---

## ⚠️ Anti-Padrões (Proibidos)

### ❌ Proibido em C#
```csharp
// ❌ Nomes em português para classes
public class UsuarioServico { }

// ❌ Notação húngara
public string strNome;
public int intId;

// ❌ Prefixo underscore em propriedades públicas
public string _Nome { get; set; }

// ❌ Abreviações obscuras
public class UsrRep { }
```

### ❌ Proibido em Oracle
```sql
-- ❌ Nomes em camelCase
CREATE TABLE usuario (idUsuario NUMBER);

-- ❌ Sem underscore em colunas
CREATE TABLE PAGINA (idpagina NUMBER);

-- ❌ Nomes em minúsculo
create table perfil (id number);
```

---

> **Regra de Ouro:** Se uma convenção não estiver listada aqui, siga o estilo predominante no código existente do projeto.