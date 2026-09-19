# 📜 CLAUDE.md — Governança Central do Projeto GestãoNew

## Diretiva de Execução Multi-Agente

Este projeto utiliza uma orquestração multi-agente para desenvolvimento. Abaixo estão os agentes disponíveis e quando cada um deve ser ativado:

| Agente | Persona | Quando Ativar |
|--------|---------|---------------|
| `agents/architect.md` | Arquiteto de Software | Início de novas features, decisões estruturais, definição de contratos |
| `agents/backend-engineer.md` | Engenheiro Backend .NET | Implementação de endpoints, services, repositórios, validações |
| `agents/database-engineer.md` | Engenheiro de Dados | Modelagem de tabelas, queries Dapper, procedures Oracle, migrations |
| `agents/qa-engineer.md` | QA / Testes | Criação de testes unitários/integração, validação de regras |
| `agents/devops-engineer.md` | DevOps | Configuração de deploy, CI/CD, Docker, infraestrutura |

## Artefatos de Controle

| Arquivo | Propósito | Atualizar Quando |
|---------|-----------|------------------|
| `CLAUDE.md` | Constituição do projeto | Contratos arquiteturais mudarem |
| `PROGRESS.md` | Snapshot de sessão + falhas | Final de cada sessão |
| `TASKS.md` | Backlog operacional granular | Checklist concluído ou nova tarefa surgir |
| `agents/*.md` | Instruções de persona | Novo padrão técnico for definido |
| `commands/*.md` | Prompts reutilizáveis | Novo padrão de tarefa for identificado |
| `skills/*.md` | Guias técnicos | Nova tecnologia for adotada no projeto |

## Protocolo de Execução Obrigatório

⚠️ **Sempre siga esta ordem antes de escrever qualquer código:**

1. **LEIA** `PROGRESS.md` → para saber onde parou e o que já falhou
2. **LEIA** `TASKS.md` → para identificar a próxima tarefa a executar
3. **ATIVE** o agente especialista correspondente → carregue o `/agents/` adequado
4. **CONSULTE** `/skills/` aplicáveis → se a tarefa envolver ORM, Oracle, etc.
5. **EXECUTE** a tarefa → seguindo os contratos abaixo
6. **VALIDE** → rode `dotnet build` para garantir que compila
7. **ATUALIZE** `TASKS.md` → marque o que foi feito
8. **ATUALIZE** `PROGRESS.md` → registre o progresso da sessão e o que falhou

## Frontend (Empresa.Web)

- **Stack real:** React 18 + TypeScript + Vite + Ant Design 5 + Zustand + React Router DOM 6 + Axios + dayjs
- **Comandos:** `npm run dev` (dev), `npm run build` (build), `npm run lint` (lint), `npm run test` (vitest)
- **Estrutura:** `src/modules/<modulo>/{components,services,hooks,types.ts,<Modulo>Page.tsx}`
- **⚠️ Proibido:** usar a stack antiga (Vue/PrimeVue/Pinia) — o projeto foi migrado para React.

## Contratos Arquiteturais Estritos (.NET 9 + Dapper + Oracle)

1. **📐 Clean Architecture** — A solução deve manter 4 camadas: `Api` (apresentação), `Data` (infra/dados), `Util` (utilitários cross-cutting), `Worker` (background). Nenhuma camada pode referenciar outra que não seja a imediatamente abaixo.

2. **🗄️ Dapper + Oracle** — Toda comunicação com banco deve usar Dapper. Proibições: Entity Framework, ADO.NET puro (fora Dapper), stored procedures com lógica de negócio.

3. **🔗 Injeção de Dependência nativa** — Usar apenas DI do `Microsoft.Extensions.DependencyInjection`. Nenhum container IoC de terceiros (Autofac, Ninject, etc.).

4. **📦 Nomenclatura** — Namespaces: `Empresa.Camada.Subdominio`. Ex: `Empresa.Data.Repositories`, `Empresa.Api.Controllers`. Classes em inglês, comentários e documentação em português.

5. **🔒 Async/Await** — Toda operação de I/O (banco, arquivo, rede) deve ser `async Task`. Proibido `.Result` ou `.Wait()`.

6. **📋 Models vs DTOs** — `Empresa.Data.Models` contém as entidades de banco. Controllers/Endpoints recebem DTOs específicos, nunca as entidades diretamente.

7. **✅ Tratamento de Erros** — Endpoints devem retornar `IResult` padronizado: `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`. Exceções não tratadas devem ser capturadas por middleware global.

8. **🛡️ Senhas** — Campo `Senha` em `Usuario` deve armazenar hash (BCrypt ou PBKDF2). Jamais armazenar texto plano.

## Checklists Universais

### ✅ Pré-tarefa
- [ ] Li `PROGRESS.md` para evitar repetir abordagens que já falharam
- [ ] Identifiquei no `TASKS.md` qual tarefa vou executar
- [ ] Carreguei o agente adequado em `agents/`
- [ ] Consultei skills relevantes em `skills/`
- [ ] Entendi os contratos arquiteturais acima

### ✅ Pós-tarefa
- [ ] Código compila sem warnings (`dotnet build`)
- [ ] Testes unitários relevantes passam (`dotnet test`)
- [ ] `TASKS.md` atualizado com checkboxes marcados
- [ ] `PROGRESS.md` atualizado: progresso + falhas + perguntas em aberto
- [ ] Código segue os 8 contratos arquiteturais acima

## Restrições Globais

- **Nunca** crie classes ou arquivos soltos na raiz do projeto. Tudo deve estar em um dos projetos da solution.
- **Nunca** use `Sync` para operações de I/O.
- **Nunca** compartilhe conexões de banco entre requisições sem usar `DbConnection` por escopo.
- **Sempre** use `using` ou `await using` para disposição de recursos não gerenciados.
- **Nunca** commite `.env`, `appsettings.*.local`, `secrets.json`, chaves ou binários (`bin/`, `obj/`).
- **Nunca** altere migrations/procedures Oracle destrutivas sem aprovação do `database-engineer`.
- **Nunca** use a stack frontend desatualizada (Vue/PrimeVue/Pinia) — o projeto é React.
