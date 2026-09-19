# 🏢 GestaoNew API

API modernizada do sistema **TreisTecnovin** — Gestão Empresarial.

## Stack Tecnológica

| Componente | Tecnologia |
|------------|-----------|
| **Framework** | .NET 9 |
| **API** | ASP.NET Core Minimal APIs |
| **ORM** | Dapper 2.1.79 |
| **Banco** | Oracle (Oracle.ManagedDataAccess.Core 23.26.300) |
| **Autenticação** | JWT Bearer + BCrypt |
| **Logging** | Serilog |
| **Documentação** | Swagger / OpenAPI |
| **Testes** | xUnit + Moq |

## Estrutura da Solution

```
Empresa.sln
├── Empresa.Api/         # API REST (Minimal APIs)
│   ├── Endpoints/       # Endpoints por domínio
│   ├── Services/        # Regras de negócio
│   ├── DTOs/            # Request/Response DTOs
│   ├── Middleware/       # Exception handling
│   └── Program.cs       # Configuração principal
├── Empresa.Data/        # Camada de dados
│   ├── Models/          # Entidades Oracle
│   ├── Repositories/    # Acesso a dados (Dapper)
│   └── DbSession.cs     # Gerenciamento de conexão
├── Empresa.Util/        # Utilitários
├── Empresa.Worker/      # Background service
└── Empresa.Tests/       # Testes unitários
```

## Pré-requisitos

- .NET SDK 9.0
- Oracle Database (acessível via network)
- Visual Studio 2022 / VS Code

## Configuração

### 1. Connection String Oracle

Edite `Empresa.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Oracle": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));User Id=seu_usuario;Password=sua_senha;"
  }
}
```

Para desenvolvimento seguro, use User Secrets:

```bash
dotnet user-secrets init --project Empresa.Api
dotnet user-secrets set "ConnectionStrings:Oracle" "Data Source=...;User Id=...;Password=..."
```

### 2. JWT Secret

O `appsettings.json` já contém uma chave JWT de desenvolvimento. Para produção, configure via variável de ambiente:

```bash
# Windows
setx Jwt__Secret "sua-chave-secreta-de-32-caracteres-ou-mais"

# Linux / Docker
export Jwt__Secret="sua-chave-secreta-de-32-caracteres-ou-mais"
```

## Execução

```bash
# Restaurar pacotes
dotnet restore

# Build
dotnet build

# Executar API
cd Empresa.Api
dotnet run

# Executar Worker
cd Empresa.Worker
dotnet run

# Executar testes
dotnet test
```

## Swagger UI

Acesse: [http://localhost:5000/swagger](http://localhost:5000/swagger)

## Endpoints da API

### Autenticação
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/auth/login` | Login (retorna JWT) |
| POST | `/api/v1/auth/refresh` | Renovar token |
| POST | `/api/v1/auth/alterar-senha` | Alterar senha |

### Usuários
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/usuarios` | Listar usuários |
| GET | `/api/v1/usuarios/{id}` | Buscar por ID |
| POST | `/api/v1/usuarios` | Criar usuário |
| PUT | `/api/v1/usuarios/{id}` | Atualizar |
| DELETE | `/api/v1/usuarios/{id}` | Excluir (lógico) |

### Páginas / Menu
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/paginas` | Listar páginas |
| GET/POST/PUT/DELETE | `/api/v1/paginas/{id}` | CRUD páginas |

### Perfis
| Método | Rota | Descrição |
|--------|------|-----------|
| GET/POST/PUT/DELETE | `/api/v1/perfis` | CRUD perfis |

### Estabelecimentos
| Método | Rota | Descrição |
|--------|------|-----------|
| GET/POST/PUT/DELETE | `/api/v1/estabelecimentos` | CRUD estabelecimentos |

### Compras
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/compras/resumo-anual` | Resumo anual (4 cursores) |
| GET | `/api/v1/compras/comite` | Comitê de compras NF |
| GET | `/api/v1/compras/previsto-realizado` | Previsto vs Realizado |
| GET | `/api/v1/compras/progressao-preco` | Progressão de preços |
| GET | `/api/v1/compras/cfop` | CFOPs de transferência |
| POST/DELETE | `/api/v1/compras/cfop` | CRUD CFOP |
| GET | `/api/v1/compras/centro-custo` | Centro de custo (3 níveis) |

### Financeiro
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/financeiro/posicao` | Posição financeira (`?versao=new\|legado\|sreal`) |
| GET | `/api/v1/financeiro/posicao/resumo` | Resumo (3 cursores) |
| GET | `/api/v1/financeiro/posicao/semanal` | Posição semanal |
| GET | `/api/v1/financeiro/fluxo-caixa` | Fluxo de caixa |
| GET | `/api/v1/financeiro/dre` | DRE (`?versao=padrao\|homologado\|out`) |
| GET | `/api/v1/financeiro/projecao` | Projeção financeira |
| GET/POST/DELETE | `/api/v1/financeiro/ajustes` | Ajustes financeiros |
| GET/POST | `/api/v1/financeiro/portadores` | Portadores |

### Prazo Médio
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/prazo-medio/recebimento/mensal` | Nível 1: mensal |
| GET | `/api/v1/prazo-medio/recebimento/pessoa` | Nível 2: pessoa |
| GET | `/api/v1/prazo-medio/recebimento/documentos` | Nível 3: documentos |
| GET | `/api/v1/prazo-medio/pagamento/mensal` | Pagamento mensal |
| GET | `/api/v1/prazo-medio/pagamento/pessoa` | Pagamento pessoa |
| GET | `/api/v1/prazo-medio/pagamento/documentos` | Documentos pagamento |

### Vendas
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/vendas/analise` | Análise de vendas |
| GET | `/api/v1/vendas/ranking-clientes` | Ranking de clientes |
| GET | `/api/v1/vendas/plano-vendas` | Plano vs resultado |
| GET | `/api/v1/vendas/comercial-mi` | Comercial MI |

### Módulo Agrícola (Fase 19)
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/v1/agricola/metas` | Lista metas de compra (ORDER BY SAFRA DESC) | JWT + `cadastroSafraMeta` |
| GET | `/api/v1/agricola/metas/empresas` | Domínio fixo de empresas (4 itens) | JWT + `cadastroSafraMeta` |
| POST | `/api/v1/agricola/metas` | Cria meta (validações server-side) | JWT + `cadastroSafraMeta` |
| PUT | `/api/v1/agricola/metas/{id}` | Atualiza meta | JWT + `cadastroSafraMeta` |
| DELETE | `/api/v1/agricola/metas/{id}` | Exclui meta (físico + log Serilog) | JWT + `cadastroSafraMeta` |
| GET | `/api/v1/agricola/compras-frutas?data=` | Painel Compras Frutas (grid dinâmica) | JWT + `comprasFrutasPorEmpresas` |
| GET | `/api/v1/agricola/compras-frutas/detalhamento` | Drill-down nível 1 (variedade/grau) | JWT + `comprasFrutasPorEmpresas` |
| GET | `/api/v1/agricola/compras-frutas/notas-fiscais` | Drill-down nível 2 (NFs) | JWT + `comprasFrutasPorEmpresas` |

> ⚠️ **Nota de quebra de contrato:** O endpoint `GET /api/v1/agricola/compras-frutas` foi **reescrito** na Fase 19. O stub antigo (Fase 8) usava assinatura incorreta (`empresa`/`safra`), string mágica de package e model incompatível. O novo endpoint recebe apenas `?data=ISO8601` e retorna contrato dinâmico (`colunas` + `linhas`). Ver `PRD_AGRICOLA.md §5.3`.

### Módulos Secundários
| Método | Rota | Descrição |
|--------|------|-----------|
| GET/PUT | `/api/v1/calendario` | Calendário financeiro |
| GET/POST/DELETE | `/api/v1/agrupamentos` | Agrupamentos DRE |
| GET | `/api/v1/dba/sessoes` | Sessões Oracle 🔒 |
| DELETE | `/api/v1/dba/sessoes/{sid}/{serial}` | Matar sessão 🔒 |
| GET | `/api/v1/dba/locks` | Locks ativos 🔒 |

### Health Checks
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/health` | Health check básico |
| GET | `/api/health/database` | Conexão Oracle |

## Autenticação

1. Faça POST em `/api/v1/auth/login` com `{ "login": "admin", "senha": "123" }`
2. Copie o `token` retornado
3. Clique em "Authorize" no Swagger e cole: `Bearer {token}`

## Pacotes Oracle Mapeados

O repositório implementa chamadas para **~25 packages Oracle**:
- `packageCompraNf`, `packageFinanceiro`, `packageFinanceiroNew`
- `PKG_POSICAO_FINANCEIRA_SREAL`, `packagePlanejamentoDre`
- `packageVendas`, `packageBi`, `packageComercial`
- `PKG_PRAZO_MEDIO`, `PKG_FINANCEIRO`, `PKG_PRAZO_MEDIO_PMZ`
- `pkg_bi_compras` (agrícola — Compras Frutas: 3 procedures REF CURSOR)
- Consultas diretas em `V$SESSION`, `AGRUPAMENTO_DRE`, `CALENDARIO_FINANCEIRO`

## Contratos Arquiteturais

Consulte os documentos em `docs/`:
- `architecture.md` — Clean Architecture
- `api-patterns.md` — Minimal APIs
- `data-layer.md` — Dapper + Oracle
- `oracle-connection.md` — Parâmetros de conexão Oracle (ODP.NET)
- `deploy-producao.md` — Manual passo a passo de deploy em produção (VPS + FortiClient VPN + Docker + SSL)
- `padroes-paineis.md` — Checklist obrigatório de acesso, conexão e carregamento para novos painéis
- `auditoria-aprendizado-paineis.md` — Auditoria dos incidentes recorrentes e incorporação das correções
- `error-handling.md` — Exception middleware
- `security.md` — JWT + BCrypt
- `testing.md` — Estratégia de testes
