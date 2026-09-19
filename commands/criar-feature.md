# Command: Criar Nova Feature

## Quando usar
Sempre que precisar implementar uma nova funcionalidade no sistema (novo endpoint, novo módulo, nova entidade).

## Prompt para execução

```markdown
Você é um Engenheiro de Software implementando uma nova feature no projeto GestãoNew (.NET 9, Dapper, Oracle, React 18).

## Contexto
- [Descreva a feature: o que faz, quais entidades envolve, regras de negócio]
- Stack: .NET 9, Minimal API, Dapper, Oracle, React 18, TypeScript, Ant Design, Zustand e Axios

## Tarefas

### 1. Análise
- [ ] Identificar se a feature precisa de: nova model, novo repositório, novo service, novos endpoints
- [ ] Verificar se as models existentes (`Empresa.Data.Models`) já cobrem a necessidade
- [ ] Verificar se a tabela Oracle já existe ou precisa ser criada
- [ ] Definir uma única `chaveControle` canônica e confirmar sua existência em `ACESSO_CADASTRO_PAGINA`
- [ ] Registrar o contrato API/frontend: rota, query/body, DTO de resposta, estados vazio/erro/loading e códigos HTTP
- [ ] Consultar `docs/padroes-paineis.md`, `docs/GUIA_MENU.md` e `docs/oracle-connection.md`

### 2. Camada de Dados (se aplicável)
- [ ] Criar/atualizar model em `Empresa.Data.Models`
- [ ] Criar interface de repositório em `Empresa.Data.Repositories`
- [ ] Criar implementação Dapper do repositório
- [ ] Registrar DI em `Program.cs` (DbSession + repositório)
- [ ] Usar conexão scoped; abrir com `OpenAsync()` quando houver `OracleCommand`/REF CURSOR e dispor reader/command
- [ ] Validar schema, package e parâmetros no Oracle real; se indisponível, registrar bloqueio e criar teste de contrato sem declarar validação real concluída

### 3. Camada de Serviço (se aplicável)
- [ ] Criar DTOs em `Empresa.Api.DTOs`
- [ ] Criar interface do service em `Empresa.Api.Services`
- [ ] Criar implementação do service com regras de negócio
- [ ] Registrar DI em `Program.cs`
- [ ] Aplicar gate server-side usando exatamente a `chaveControle` canônica

### 4. Camada de Apresentação
- [ ] Criar endpoints Minimal API em `Empresa.Api.Endpoints`
- [ ] Seguir padrão: `Results.Ok`, `Results.NotFound`, `Results.BadRequest`
- [ ] Mapear endpoints no `Program.cs` (se necessário)
- [ ] Adicionar `.RequireAuthorization()` e documentar 401/403/500; não converter erro de banco em 401/403

### 5. Frontend (se aplicável)
- [ ] Usar exclusivamente `Empresa.Web/src/lib/api.ts`; nunca criar outra instância Axios
- [ ] Criar tipos, service e hook/store no módulo; tratar loading, erro e estado vazio
- [ ] Registrar a mesma `chaveControle` em `ROUTE_MAP` e a rota lazy em `App.tsx`
- [ ] Envolver a rota com `MenuGuard`; o backend continua sendo a autoridade final
- [ ] Exibir mensagem acionável para timeout/500 e preservar recarga manual; auto-refresh não pode sobrepor requisições

### 6. Validação
- [ ] `dotnet build` sem erros
- [ ] `dotnet test` com casos de sucesso, vazio, falha de repositório e gate sem permissão
- [ ] Testar via Swagger: sem JWT = 401, perfil sem permissão = 403, sucesso = contrato esperado, falha Oracle = 500/503 observável
- [ ] `npm run lint`, `npm run test` e `npm run build` no frontend
- [ ] Testar integração login → menu → rota → carregamento e acesso direto sem permissão
- [ ] Validar `GET /api/health/database` no ambiente integrado
- [ ] Atualizar `TASKS.md`
- [ ] Atualizar `PROGRESS.md` com o progresso
```

## Regras
- Siga os 8 contratos arquiteturais de `CLAUDE.md`
- Consulte `skills/dapper-orm.md` para padrões de repositório
- Consulte `skills/clean-architecture-dotnet.md` para organização de camadas
- Consulte `skills/oracle-procedures-dapper.md` quando houver REF CURSOR
- Consulte `skills/react-frontend-patterns.md` para painéis React
- Uma feature de painel não está concluída apenas porque compila: acesso, contrato, carregamento e Oracle integrado devem estar validados ou explicitamente bloqueados
