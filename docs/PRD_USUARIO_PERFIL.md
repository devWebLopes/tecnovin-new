# PRD — Modernização dos Painéis de Usuário e Perfil
### GestaoNew · Módulo de Acesso · v1.0
> **Data:** 2026-07-30  
> **Arquiteto responsável:** Persona `architect.md`  
> **Skill ativa:** `workflow-enforcer.md`  
> **Fonte primária:** `docs/usuario_regra.md` (engenharia reversa do legado)

---

## 1. Visão Geral da Modernização

### 1.1 Contexto

O sistema legado foi desenvolvido em **ASP.NET WebForms** (.NET Framework) com **DevExpress v16.2** e **Oracle DB** acessado via `System.Data.OracleClient` (biblioteca descontinuada). Os painéis de **Cadastro de Perfil** e **Cadastro de Usuário** vivem em:

- `Treis.Web/interna/acesso/CadastroPerfil.aspx`
- `Treis.Web/interna/acesso/CadastroUsuario.aspx`

Ambos utilizam o Data Access Object `DaoAcesso.cs` que mistura lógica de negócio, acesso a dados e construção de SQL por concatenação de string — uma violação grave de segurança (SQL Injection) e manutenibilidade.

### 1.2 Objetivo desta Entrega

Implementar, no projeto **GestaoNew** (.NET 9 + Minimal API + Dapper + Oracle), os endpoints de backend e os modelos de dados necessários para que o frontend (React/TypeScript) reproduza fielmente as funcionalidades dos dois painéis, com as seguintes melhorias obrigatórias:

| Problema Legado | Correção na Nova Stack |
|---|---|
| Senha em texto puro | Hash BCrypt obrigatório |
| SQL por concatenação (SQLi) | Queries parametrizadas com Dapper |
| Sessão ASP.NET | JWT Bearer (já implementado na Fase 2) |
| DevExpress grid/popup | API REST + frontend React |
| `System.Data.OracleClient` (descontinuado) | `Oracle.ManagedDataAccess.Core` v23 |
| INSERT sem verificação de duplicidade em perfil_pagina | MERGE Oracle ou verificação antes de INSERT |

### 1.3 Estado Atual no GestaoNew

Conforme `PROGRESS.md` (29/07/2026), a **Fase 3 (Módulo Acesso/Usuários)** está 100% concluída com os repositórios e serviços básicos criados. Contudo, a análise detalhada de `docs/usuario_regra.md` revela que os painéis completos de **Gestão de Perfil** e **Gestão de Usuário** — incluindo os subfluxos de permissões de páginas e vínculos com estabelecimentos — requerem endpoints e lógica adicionais que não foram cobertos na fase anterior.

Este PRD especifica exatamente o que está **faltando** e o que precisa ser **revisado**.

---

## 2. Escopo dos Painéis

### 2.1 Painel: Cadastro de Perfil (`cadastroPerfil`)

**Funcionalidades a implementar/completar:**

1. **CRUD de Perfis** — listagem paginada, criação, edição inline e exclusão com confirmação
2. **Popup de Permissões de Páginas** — carregar árvore hierárquica de páginas e salvar vínculos `perfil ↔ páginas`
3. **Validação de permissão de acesso** à tela via chave `cadastroPerfil`

### 2.2 Painel: Cadastro de Usuário (`cadastroUsuario`)

**Funcionalidades a implementar/completar:**

1. **CRUD de Usuários** — listagem ordenada por NOME, campos mascarados de senha, dropdown de perfis
2. **Popup de Vínculo com Empresas/Estabelecimentos** — árvore hierárquica empresa→estabelecimento com pre-check dos vínculos existentes
3. **Salvar vínculos Usuário ↔ Estabelecimento** com verificação de duplicidade antes do INSERT
4. **Funcionalidade Alterar Senha** — fluxo obrigatório quando `ATUALIZA_SENHA = 'S'` (já existe no `AuthEndpoints`, mas precisa ser verificado)

---

## 3. Mapeamento de Regras de Negócio

### 3.1 Regras Globais de Autenticação (já implementadas — validar)

| Regra | Implementação Esperada |
|---|---|
| Token JWT obrigatório em todos os endpoints do módulo | `[Authorize]` no grupo `/api/v1/perfis` e `/api/v1/usuarios` |
| Verificação de permissão de página por `CHAVE_CONTROLE` | `GET /api/v1/acesso/pagina?chave={chave}` já existe em `PaginaEndpoints` |
| `ATUALIZA_SENHA = 'S'` → forçar troca | `POST /api/v1/auth/alterar-senha` já existe em `AuthEndpoints` |

### 3.2 Regras de Negócio — Perfil

#### RN-PERF-01: CRUD de Perfis
- `SELECT ID_PERFIL, DESCRICAO FROM ACESSO_CADASTRO_PERFIL` — sem filtro de ATIVO (perfis não têm exclusão lógica)
- `DESCRICAO` é obrigatória
- `ID_PERFIL` é gerado por **Sequence/Trigger Oracle** — a aplicação NÃO gera o ID; o INSERT não inclui `ID_PERFIL` ou usa `RETURNING`
- DELETE físico via `DELETE FROM ACESSO_CADASTRO_PERFIL WHERE ID_PERFIL = :id`
- Paginação: 50 registros por página (padrão legado, manter configurável)

#### RN-PERF-02: Árvore de Páginas para Permissões
- Carrega **todas as páginas** com `ATIVO = 'S'` da tabela `ACESSO_CADASTRO_PAGINA`
- Estrutura hierárquica:
  - **Nós raiz:** `ID_PAGINA_PAI IS NULL`, ordenados por `ORDEM`
  - **Nós filhos:** `ID_PAGINA_PAI = {id_pai}`, ordenados por `ORDEM`
- Páginas **já vinculadas** ao perfil (`ACESSO_PERFIL_PAGINA`) aparecem pré-marcadas

> **Abordagem Oracle recomendada:** usar `CONNECT BY PRIOR` para retornar a hierarquia em uma única query ao invés de múltiplas trips ao banco.

#### RN-PERF-03: Salvar Permissões do Perfil
- **Nó MARCADO (checked):** INSERT em `ACESSO_PERFIL_PAGINA (ID_PERFIL, ID_PAGINA)`
  - ⚠️ **Correção obrigatória:** legado não verifica duplicidade antes do INSERT — usar `MERGE INTO` Oracle para ser idempotente
- **Nó DESMARCADO (unchecked):** DELETE de `ACESSO_PERFIL_PAGINA WHERE ID_PERFIL = :idPerfil AND ID_PAGINA = :idPagina`
- Operação atômica: toda a árvore é processada em uma única transação

### 3.3 Regras de Negócio — Usuário

#### RN-USR-01: CRUD de Usuários
- Listagem: `ORDER BY NOME` — obrigatório
- Colunas retornadas pela API: `ID_USUARIO, NOME, LOGIN, ID_PERFIL, ATIVO, ATUALIZA_SENHA, DATA_HORA_ULTIMO_ACESSO, QUANTIDADE_ACESSO`
- **Senha NUNCA retornada na listagem/response** (segurança)
- **INSERT:** `QUANTIDADE_ACESSO` default `0`, `ATUALIZA_SENHA` default `'N'`
- **UPDATE:** NÃO atualiza `DATA_HORA_ULTIMO_ACESSO` nem `QUANTIDADE_ACESSO` (apenas o login faz isso via `SalvaAcessoUsuario`)
- **Senha:** BCrypt hash obrigatório antes de persistir (correção crítica do legado)
- Validações:
  - `NOME`: obrigatório
  - `LOGIN`: obrigatório, max 50 chars, **único** na tabela
  - `SENHA`: obrigatório na criação, max 50 chars (hash BCrypt pode ultrapassar — ajustar coluna se necessário ou truncar hash)
  - `ID_PERFIL`: obrigatório, FK válida em `ACESSO_CADASTRO_PERFIL`
  - `ATIVO`: obrigatório, valores aceitos: `'S'` ou `'N'`

#### RN-USR-02: Dropdown de Perfis (auxiliar)
- `SELECT ID_PERFIL, DESCRICAO FROM ACESSO_CADASTRO_PERFIL ORDER BY DESCRICAO`
- Usado pelo frontend para popular o campo `ID_PERFIL` no formulário de usuário

#### RN-USR-03: Árvore de Empresas/Estabelecimentos
- Fonte: View Oracle `VW_ESTABELECIMENTO_NEW`
- Query: `SELECT EMPRESA, ESTABELECIMENTO, DESCRITIVO FROM VW_ESTABELECIMENTO_NEW`
- Mapeamento de campos:
  - `EMPRESA` → `CdEmpresa`
  - `ESTABELECIMENTO` → `CdEstabelecimento`
  - `DESCRITIVO` → texto até primeiro espaço = `DsEmpresa`; texto após `" - "` = `DsEstabelecimento`
- Estrutura da árvore:
  - **Nó pai (empresa):** registros onde `CdEstabelecimento == 1`
  - **Nós filhos (estabelecimentos):** todos com mesmo `CdEmpresa`, ordenados por `CdEstabelecimento`
- Vínculos existentes consultados em `ACESSO_USUARIO_EMPRESA_ESTAB`

#### RN-USR-04: Salvar Vínculo Usuário-Estabelecimento
- **Nó MARCADO:**
  - Verificar existência: `SELECT COUNT(*) FROM ACESSO_USUARIO_EMPRESA_ESTAB WHERE ID_USUARIO=:u AND CD_EMPRESA=:e AND CD_ESTABELECIMENTO=:est`
  - Se não existir: INSERT
- **Nó DESMARCADO:**
  - Verificar existência (mesmo SELECT)
  - Se existir: DELETE
- Apenas nós folha (estabelecimentos) são processados; nós pai (empresa) são ignorados
- Operação **bulk**: frontend envia lista de vínculos a manter; backend sincroniza

> **Melhoria recomendada (vs legado):** em vez de processar nó a nó com N queries, receber do frontend dois arrays: `vincularIds` e `desvincularIds`, e executar em batch.

### 3.4 Regra de Segurança Crítica

> [!CAUTION]
> **RN-SEG-01:** A senha JAMAIS deve ser:
> - Retornada em qualquer response de listagem ou detalhe
> - Armazenada sem hash BCrypt
> - Recebida sem validação mínima de comprimento (min 6 chars na criação)
>
> O campo `SENHA VARCHAR2(50)` no Oracle pode ser insuficiente para um hash BCrypt (60 chars). O **database-engineer** deve verificar e aumentar para `VARCHAR2(60)` se necessário, ou usar o hash truncado com `SUBSTR`.

---

## 4. Contratos de API

### 4.1 Namespace e Prefixo

```
/api/v1/perfis
/api/v1/usuarios
```

Ambos os grupos exigem autenticação JWT Bearer (`RequireAuthorization()`).

---

### 4.2 Endpoints de Perfil

#### `GET /api/v1/perfis`
**Descrição:** Lista todos os perfis (sem paginação server-side inicial; frontend pagina).  
**Query params:** `search?: string` (filtro por DESCRICAO)  
**Response 200:**
```json
[
  { "idPerfil": 1, "descricao": "Administrador" },
  { "idPerfil": 2, "descricao": "Operador" }
]
```

#### `POST /api/v1/perfis`
**Descrição:** Cria um novo perfil.  
**Request body:**
```json
{ "descricao": "Novo Perfil" }
```
**Response 201:** `{ "idPerfil": 10, "descricao": "Novo Perfil" }`  
**Response 422:** `{ "errors": ["Descrição é obrigatória"] }`

#### `PUT /api/v1/perfis/{id}`
**Descrição:** Atualiza a descrição de um perfil.  
**Request body:** `{ "descricao": "Perfil Editado" }`  
**Response 204:** No Content  
**Response 404:** Not Found

#### `DELETE /api/v1/perfis/{id}`
**Descrição:** Exclui fisicamente um perfil.  
**Response 204:** No Content  
**Response 404:** Not Found  
**Response 409:** Conflict (se perfil tiver usuários vinculados)

#### `GET /api/v1/perfis/{id}/paginas`
**Descrição:** Retorna a árvore hierárquica de páginas com flag indicando quais estão vinculadas ao perfil.  
**Response 200:**
```json
[
  {
    "idPagina": 1,
    "tituloMenu": "Acesso",
    "ordem": 1,
    "idPaginaPai": null,
    "vinculado": false,
    "filhos": [
      {
        "idPagina": 2,
        "tituloMenu": "Perfil",
        "chaveControle": "cadastroPerfil",
        "ordem": 1,
        "idPaginaPai": 1,
        "vinculado": true,
        "filhos": []
      }
    ]
  }
]
```

#### `PUT /api/v1/perfis/{id}/paginas`
**Descrição:** Sincroniza as permissões de páginas de um perfil (operação idempotente).  
**Request body:**
```json
{
  "vincularIds": [2, 5, 8],
  "desvincularIds": [3, 7]
}
```
**Response 204:** No Content  
**Response 404:** Perfil não encontrado  
**Response 422:** IDs de página inválidos

---

### 4.3 Endpoints de Usuário

#### `GET /api/v1/usuarios`
**Descrição:** Lista todos os usuários ordenados por NOME.  
**Query params:** `search?: string`, `ativo?: string` ('S'/'N')  
**Response 200:**
```json
[
  {
    "idUsuario": 1,
    "nome": "João Silva",
    "login": "joao.silva",
    "idPerfil": 2,
    "descricaoPerfil": "Operador",
    "ativo": "S",
    "atualizaSenha": "N",
    "dataHoraUltimoAcesso": "2026-07-29T14:30:00",
    "quantidadeAcesso": 42
  }
]
```
> ⚠️ **O campo `senha` NUNCA aparece no response.**

#### `POST /api/v1/usuarios`
**Descrição:** Cria um novo usuário.  
**Request body:**
```json
{
  "nome": "João Silva",
  "login": "joao.silva",
  "senha": "senhaForte123",
  "idPerfil": 2,
  "ativo": "S",
  "atualizaSenha": "N"
}
```
**Response 201:** `UsuarioResponse` do usuário criado  
**Response 409:** Login já cadastrado  
**Response 422:** Campos obrigatórios ausentes

#### `PUT /api/v1/usuarios/{id}`
**Descrição:** Atualiza dados de um usuário. Se `senha` for enviada, aplica novo hash BCrypt.  
**Request body:** igual ao POST (senha opcional no update)  
**Response 204:** No Content  
**Response 404:** Not Found

#### `DELETE /api/v1/usuarios/{id}`
**Descrição:** Exclusão lógica — seta `ATIVO = 'N'`.  
**Response 204:** No Content  
**Response 404:** Not Found

#### `GET /api/v1/usuarios/{id}/estabelecimentos`
**Descrição:** Retorna a árvore hierárquica de empresas/estabelecimentos com flag de vínculo.  
**Response 200:**
```json
[
  {
    "cdEmpresa": 1,
    "dsEmpresa": "TecnoVin",
    "estabelecimentos": [
      {
        "cdEstabelecimento": 1,
        "dsEstabelecimento": "Matriz",
        "vinculado": true
      },
      {
        "cdEstabelecimento": 2,
        "dsEstabelecimento": "Filial SP",
        "vinculado": false
      }
    ]
  }
]
```

#### `PUT /api/v1/usuarios/{id}/estabelecimentos`
**Descrição:** Sincroniza vínculos usuário ↔ estabelecimentos.  
**Request body:**
```json
{
  "vincularEstabelecimentos": [
    { "cdEmpresa": 1, "cdEstabelecimento": 2 }
  ],
  "desvincularEstabelecimentos": [
    { "cdEmpresa": 1, "cdEstabelecimento": 3 }
  ]
}
```
**Response 204:** No Content

---

### 4.4 Endpoints Auxiliares (Transversais)

#### `GET /api/v1/perfis/lista-simples`
**Descrição:** Retorna lista simplificada de perfis para dropdown de seleção no formulário de usuário.  
**Response 200:** `[{ "idPerfil": 1, "descricao": "Administrador" }]`

---

## 5. Modelos de Dados Necessários

### 5.1 DTOs a criar/completar em `Empresa.Api/DTOs/`

| Arquivo | Conteúdo | Ação |
|---|---|---|
| `Request/PerfilRequest.cs` | `Descricao` | **CRIAR** |
| `Response/PerfilResponse.cs` | `IdPerfil`, `Descricao` | Verificar se existe; **CRIAR/COMPLETAR** |
| `Request/PerfilPaginasRequest.cs` | `VincularIds[]`, `DesvincularIds[]` | **CRIAR** |
| `Response/PaginaTreeResponse.cs` | Estrutura hierárquica com `Filhos[]` e `Vinculado` | **CRIAR** |
| `Request/UsuarioRequest.cs` | Todos os campos do usuário | Verificar se completo |
| `Response/UsuarioResponse.cs` | Campos + `DescricaoPerfil` (join) — SEM senha | Verificar se seguro |
| `Response/EstabelecimentoTreeResponse.cs` | Estrutura hierárquica com `Vinculado` | **CRIAR** |
| `Request/EstabelecimentoVinculoRequest.cs` | `VincularEstabelecimentos[]`, `DesvincularEstabelecimentos[]` | **CRIAR** |

### 5.2 Models a criar/completar em `Empresa.Data/Models/`

| Arquivo | Conteúdo | Ação |
|---|---|---|
| `Perfil.cs` | `IdPerfil`, `Descricao` | Verificar se existe |
| `Pagina.cs` | Todos os campos de `ACESSO_CADASTRO_PAGINA` + `Filhos` | Verificar se existe e está completo |
| `PerfilPagina.cs` | `IdPerfil`, `IdPagina` | **CRIAR** |
| `UsuarioEmpresaEstab.cs` | `IdUsuario`, `CdEmpresa`, `CdEstabelecimento` | **CRIAR** |

---

## 6. Plano de Execução Multi-Agente

### Ordem de execução e dependências

```
database-engineer ──► backend-engineer ──► qa-engineer
      ↓                      ↓                  ↓
  Verifica schema        Implementa         Valida
  e constraints          endpoints          contratos
```

> [!IMPORTANT]
> **Regra de camadas (Clean Architecture):**  
> - `database-engineer` atua APENAS em `Empresa.Data/` (Models, Repositories, Oracle)  
> - `backend-engineer` atua em `Empresa.Api/` (DTOs, Services, Endpoints) e nunca acessa `DbSession` diretamente  
> - `qa-engineer` atua em `Empresa.Tests/` e valida endpoints via chamadas de serviço mockadas  
> - **Nenhum agente** pode criar referência cruzada que viole a tabela de dependências de `docs/architecture.md`

---

### 6.1 Agente: `database-engineer.md`

**Responsabilidade:** Garantir que o schema Oracle e a camada `Empresa.Data` suportam todas as operações especificadas neste PRD.

**Tarefas delegadas:**

| ID | Tarefa | Camada | Prioridade |
|---|---|---|---|
| DB-01 | Verificar existência e estrutura das tabelas `ACESSO_CADASTRO_PERFIL`, `ACESSO_PERFIL_PAGINA`, `ACESSO_CADASTRO_PAGINA` via `SELECT` de metadados | Data | P0 |
| DB-02 | Verificar se coluna `SENHA` em `ACESSO_CADASTRO_USUARIO` tem tamanho ≥ 60 chars para suportar hash BCrypt; propor `ALTER TABLE` se necessário | Data | P0 |
| DB-03 | Criar/completar `Empresa.Data/Models/Pagina.cs` com todos os campos de `ACESSO_CADASTRO_PAGINA` | Data | P0 |
| DB-04 | Criar `Empresa.Data/Models/PerfilPagina.cs` | Data | P0 |
| DB-05 | Criar `Empresa.Data/Models/UsuarioEmpresaEstab.cs` | Data | P0 |
| DB-06 | Criar/completar `Empresa.Data/Repositories/IPerfilRepository.cs` — adicionar métodos: `GetPaginasTreeAsync(idPerfil)`, `SalvarPermissoesAsync(idPerfil, vincularIds, desvincularIds)` | Data | P0 |
| DB-07 | Implementar `PerfilRepository.cs` — query hierárquica com `CONNECT BY PRIOR`; MERGE para permissões | Data | P0 |
| DB-08 | Criar/completar `IUsuarioRepository.cs` — adicionar `GetEstabelecimentosTreeAsync(idUsuario)`, `SincronizarEstabelecimentosAsync(idUsuario, vincular[], desvincular[])` | Data | P1 |
| DB-09 | Implementar `UsuarioRepository.cs` — verificar se `GetAllAsync` retorna `JOIN` com `ACESSO_CADASTRO_PERFIL` para `DescricaoPerfil`; senhas NUNCA retornadas | Data | P0 |
| DB-10 | Aplicar `skills/oracle-best-practices.md` e `skills/dapper-orm.md` em todas as queries — parâmetros `:nome`, sem `SELECT *`, paginação `OFFSET/FETCH` | Data | P0 |

---

### 6.2 Agente: `backend-engineer.md`

**Responsabilidade:** Implementar os endpoints, DTOs e Services na camada `Empresa.Api`, consumindo os repositórios entregues pelo `database-engineer`.

**Tarefas delegadas:**

| ID | Tarefa | Camada | Prioridade |
|---|---|---|---|
| BE-01 | Criar `Empresa.Api/DTOs/Request/PerfilRequest.cs` com `DataAnnotations` | Api | P0 |
| BE-02 | Criar `Empresa.Api/DTOs/Response/PerfilResponse.cs` | Api | P0 |
| BE-03 | Criar `Empresa.Api/DTOs/Request/PerfilPaginasRequest.cs` | Api | P0 |
| BE-04 | Criar `Empresa.Api/DTOs/Response/PaginaTreeResponse.cs` (hierárquico com `List<PaginaTreeResponse> Filhos`) | Api | P0 |
| BE-05 | Verificar/completar `Empresa.Api/DTOs/Request/UsuarioRequest.cs` — todos os campos, senha opcional no update | Api | P0 |
| BE-06 | Verificar/completar `Empresa.Api/DTOs/Response/UsuarioResponse.cs` — incluir `DescricaoPerfil`, sem `Senha` | Api | P0 |
| BE-07 | Criar `Empresa.Api/DTOs/Response/EstabelecimentoTreeResponse.cs` | Api | P1 |
| BE-08 | Criar `Empresa.Api/DTOs/Request/EstabelecimentoVinculoRequest.cs` | Api | P1 |
| BE-09 | Criar/completar `Empresa.Api/Services/IPerfilService.cs` e `PerfilService.cs` — CRUD + gerência de permissões | Api | P0 |
| BE-10 | Verificar/completar `Empresa.Api/Services/IUsuarioService.cs` e `UsuarioService.cs` — BCrypt no create/update, exclusão lógica, JOIN com perfil | Api | P0 |
| BE-11 | Criar/completar `Empresa.Api/Endpoints/PerfilEndpoints.cs` — 6 endpoints conforme seção 4.2 | Api | P0 |
| BE-12 | Criar/completar `Empresa.Api/Endpoints/UsuarioEndpoints.cs` — 6 endpoints conforme seção 4.3 | Api | P0 |
| BE-13 | Registrar `PerfilEndpoints` e `UsuarioEndpoints` em `Program.cs` (verificar se já estão) | Api | P0 |
| BE-14 | Registrar `IPerfilService`, `IUsuarioService` no DI container de `Program.cs` | Api | P0 |
| BE-15 | Garantir que todos os endpoints têm `.RequireAuthorization()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()` | Api | P1 |
| BE-16 | Executar `dotnet build` ao final e confirmar 0 erros | Api | P0 |

---

### 6.3 Agente: `qa-engineer.md`

**Responsabilidade:** Garantir cobertura de testes unitários para os Services e de integração para os Endpoints do módulo de Acesso/Usuários/Perfil.

**Tarefas delegadas:**

| ID | Tarefa | Camada | Prioridade |
|---|---|---|---|
| QA-01 | Criar `Empresa.Tests/Services/PerfilServiceTests.cs` — casos: listar todos, criar válido, criar sem descrição (erro), atualizar, deletar | Tests | P0 |
| QA-02 | Criar `Empresa.Tests/Services/PerfilPermissoesTests.cs` — casos: salvar permissões (merge), remover permissões (delete), perfil inexistente (404) | Tests | P1 |
| QA-03 | Verificar/expandir `Empresa.Tests/Services/UsuarioServiceTests.cs` — adicionar: login duplicado (409), update sem hash exposição, exclusão lógica (ATIVO='N') | Tests | P0 |
| QA-04 | Criar `Empresa.Tests/Services/UsuarioEstabelecimentoTests.cs` — casos: sincronizar vínculos (vincular novo, desvincular existente), usuário inexistente (404) | Tests | P1 |
| QA-05 | Validar que nenhum `UsuarioResponse` contém campo `Senha` ou `PasswordHash` (teste de segurança) | Tests | P0 |
| QA-06 | Executar `dotnet test` e confirmar que todos os testes existentes continuam passando (regressão) | Tests | P0 |
| QA-07 | Documentar resultado dos testes em `PROGRESS.md` | Tests | P0 |

---

## 7. Critérios de Aceite

| # | Critério | Responsável |
|---|---|---|
| AC-01 | `dotnet build` sem erros e sem warnings de nivel Error | `backend-engineer` |
| AC-02 | `dotnet test` com 100% de testes passando (incluindo os novos) | `qa-engineer` |
| AC-03 | Nenhuma senha em texto puro nos testes ou responses | `qa-engineer` |
| AC-04 | Endpoints do Perfil respondem corretamente: GET lista, POST cria, PUT edita, DELETE remove, GET árvore de páginas, PUT salva permissões | `backend-engineer` |
| AC-05 | Endpoints do Usuário respondem corretamente: GET lista (sem senha), POST cria (BCrypt), PUT atualiza, DELETE exclusão lógica, GET árvore de estab, PUT sincroniza | `backend-engineer` |
| AC-06 | Queries Oracle usando parâmetros `:nome` (sem concatenação), `CONNECT BY PRIOR` para hierarquias, `MERGE` para operações idempotentes | `database-engineer` |
| AC-07 | Camadas respeitadas: nenhuma referência de `Data` para `Api`, sem instanciação direta (`new Service()`) | `backend-engineer` |
| AC-08 | `TASKS.md` e `PROGRESS.md` atualizados ao término de cada agente | Todos |

---

## 8. Restrições e Riscos

| Risco | Impacto | Mitigação |
|---|---|---|
| Coluna `SENHA VARCHAR2(50)` insuficiente para BCrypt (60 chars) | Erro de persistência | `database-engineer` verifica em DB-02 e propõe ALTER TABLE |
| `IPerfilRepository` já existe mas pode não ter os métodos de permissões | Retrabalho | `backend-engineer` verifica antes de criar novo arquivo |
| `dotnet build` pode falhar se DI não for atualizado | Bloqueio de deploy | BE-13/BE-14 são P0 |
| INSERT duplicado em `ACESSO_PERFIL_PAGINA` (bug legado) | ORA-00001 (unique constraint) | Usar MERGE Oracle conforme DB-07 |

---

## 9. Não está no Escopo deste PRD

- Implementação do **frontend React** (será PRD separado)
- Migração de dados legados para o novo schema
- Módulos distintos (Compras, Financeiro, Vendas) — já entregues
- Worker Service — já entregue (Fase 9)

---

> **Próximo passo:** O `database-engineer` deve puxar a tarefa **DB-01** do `TASKS.md` e iniciar a verificação do schema Oracle.
