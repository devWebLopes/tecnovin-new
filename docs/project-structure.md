# 📁 Estrutura do Projeto — GestãoNew

## Estrutura Completa da Solution

```
GestaoNew/
│
├── Empresa.sln                          # Solution do Visual Studio
├── CLAUDE.md                            # Governança central do projeto
├── PROGRESS.md                          # Snapshot de progresso da sessão
├── TASKS.md                             # Backlog operacional
│
├── agents/                              # Personas para agentes de IA
│   ├── README.md
│   ├── architect.md
│   ├── backend-engineer.md
│   ├── database-engineer.md
│   ├── devops-engineer.md
│   └── qa-engineer.md
│
├── commands/                            # Prompts reutilizáveis
│   ├── code-review.md
│   ├── commit.md
│   ├── criar-feature.md
│   └── fix-bug.md
│
├── skills/                              # Guias técnicos especializados
│   ├── clean-architecture-dotnet.md
│   ├── dapper-orm.md
│   └── oracle-best-practices.md
│
├── docs/                                # Documentação do projeto ← VOCÊ ESTÁ AQUI
│   ├── README.md                        # Índice principal da documentação
│   ├── architecture.md                  # Arquitetura e decisões
│   ├── project-structure.md             # Esta estrutura detalhada
│   ├── naming-conventions.md            # Convenções de nomenclatura
│   ├── getting-started.md               # Guia de setup
│   ├── coding-standards.md              # Padrões de código
│   ├── api-patterns.md                  # Padrões de API
│   ├── data-layer.md                    # Camada de dados
│   ├── error-handling.md                # Tratamento de erros
│   ├── security.md                      # Segurança
│   ├── testing.md                       # Testes
│   └── code-review.md                   # Code review
│
├── Empresa.Api/                         # API REST (Apresentação)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Empresa.Api.csproj
│   ├── Program.cs
│   └── Empresa.Api.http
│
├── Empresa.Data/                        # Camada de Dados (Infraestrutura)
│   ├── Models/
│   │   ├── Usuario.cs
│   │   └── Pagina.cs
│   ├── Empresa.Data.csproj
│   └── Class1.cs
│
├── Empresa.Util/                        # Utilitários Cross-Cutting
│   ├── Dados.cs
│   ├── Email.cs
│   ├── StringUtils.cs
│   ├── Empresa.Util.csproj
│   └── Class1.cs
│
└── Empresa.Worker/                      # Background Worker
    ├── Properties/
    │   └── launchSettings.json
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Empresa.Worker.csproj
    ├── Program.cs
    └── Worker.cs
```

## Detalhamento dos Projetos

### 1. `Empresa.Api` — API REST

**Tipo:** ASP.NET Core Web Application (.NET 9)
**Função:** Ponta de entrada HTTP, expõe endpoints REST via Minimal APIs

**Estrutura esperada:**

```
Empresa.Api/
├── Endpoints/
│   ├── UsuarioEndpoints.cs
│   ├── PaginaEndpoints.cs
│   └── ...
├── Services/
│   ├── IUsuarioService.cs
│   ├── UsuarioService.cs
│   └── ...
├── DTOs/
│   ├── Request/
│   │   ├── UsuarioRequest.cs
│   │   └── ...
│   └── Response/
│       ├── UsuarioResponse.cs
│       └── ...
├── Middleware/
│   ├── ExceptionMiddleware.cs
│   └── ...
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Empresa.Api.csproj
```

**Dependências:** `Empresa.Data`, `Empresa.Util`
**Porta (Dev):** HTTP 5139 / HTTPS 7061

### 2. `Empresa.Worker` — Background Worker

**Tipo:** Worker Service (.NET 9)
**Função:** Processamento assíncrono em background

**Estrutura:**

```
Empresa.Worker/
├── Workers/
│   ├── RelatorioWorker.cs
│   └── ...
├── Jobs/
│   ├── EnvioEmailJob.cs
│   └── ...
├── Program.cs
├── Worker.cs (template inicial)
├── appsettings.json
└── appsettings.Development.json
```

**Dependências:** `Empresa.Data`, `Empresa.Util`

### 3. `Empresa.Data` — Camada de Dados

**Tipo:** Class Library (.NET 9)
**Função:** Acesso a dados, repositórios, modelos de banco

**Estrutura:**

```
Empresa.Data/
├── Models/
│   ├── Usuario.cs
│   ├── Pagina.cs
│   └── ...
├── Repositories/
│   ├── IUsuarioRepository.cs
│   ├── UsuarioRepository.cs
│   ├── IPaginaRepository.cs
│   ├── PaginaRepository.cs
│   └── ...
├── DbSession.cs
└── Empresa.Data.csproj
```

**Dependências:** `Empresa.Util` (apenas)
**Pacotes NuGet:** `Dapper 2.1.79`, `Oracle.ManagedDataAccess.Core 23.26.300`

### 4. `Empresa.Util` — Utilitários

**Tipo:** Class Library (.NET 9)
**Função:** Código compartilhado e cross-cutting

**Estrutura:**

```
Empresa.Util/
├── Dados.cs        (constantes de formatação)
├── Email.cs        (serviço de e-mail)
├── StringUtils.cs  (utilitários de string)
└── Empresa.Util.csproj
```

**Dependências:** Nenhuma (camada mais interna)

## Arquivos Temporários / Template

Os arquivos `Class1.cs` presentes em `Empresa.Data` e `Empresa.Util` são arquivos **template** gerados automaticamente pelo Visual Studio ao criar projetos do tipo Class Library. Eles devem ser **removidos** assim que classes reais forem adicionadas aos respectivos projetos.

## Convenções de Diretórios

| Diretório | Onde Criar | Propósito |
|-----------|-----------|-----------|
| `Endpoints/` | `Empresa.Api` | Agrupar endpoints por domínio |
| `Services/` | `Empresa.Api` | Implementação de regras de negócio |
| `DTOs/Request/` | `Empresa.Api` | Objetos de entrada da API |
| `DTOs/Response/` | `Empresa.Api` | Objetos de saída da API |
| `Middleware/` | `Empresa.Api` | Middleware pipeline customizado |
| `Models/` | `Empresa.Data` | Entidades mapeadas para tabelas Oracle |
| `Repositories/` | `Empresa.Data` | Repositórios com queries Dapper |
| `Workers/` | `Empresa.Worker` | Background services |
| `Jobs/` | `Empresa.Worker` | Tarefas agendadas |

## Regras de Criação de Arquivos

1. **Nunca** crie classes soltas na raiz de um projeto — sempre dentro do subdiretório apropriado
2. **Nunca** crie arquivos na raiz da solution (fora dos projetos)
3. **Sempre** use `namespace Empresa.Camada.Subdominio;` (namespace file-scoped)
4. **Sempre** remova os `Class1.cs` template quando adicionar classes reais

---

> **Importante:** Mantenha esta estrutura atualizada conforme novos diretórios forem criados.