# Auditoria de Aprendizado dos Painéis

**Data:** 2026-08-14

## Conclusão Executiva

O projeto registrou parte relevante dos problemas e incorporou várias soluções no código, mas o aprendizado ainda era parcial. Os painéis recentes possuem padrões melhores de conexão, autenticação, autorização, carregamento e testes; porém o template canônico de novas features não exigia esses controles, havia documentação divergente e a auditoria encontrou duas regressões reais no módulo agrícola.

Após esta auditoria, o fluxo foi reforçado em `commands/criar-feature.md` e consolidado em `docs/padroes-paineis.md`. Ainda permanece necessária a validação integrada no Oracle real.

## Problemas Registrados e Soluções

| Problema recorrente | Evidência | Solução encontrada | Incorporação atual |
|---|---|---|---|
| Oracle indisponível/timeout | `PROGRESS.md` registra `ORA-50000`; `docs/oracle-connection.md` contém troubleshooting | `DbSession` scoped, health check, pooling/timeouts documentados e testes de contrato | Parcial: infraestrutura e guia existem, mas o Oracle real continua bloqueado |
| REF CURSOR sem conexão aberta | Comentário e helper em `AgricolaRepository`; regra em `skills/oracle-procedures-dapper.md` | Abrir conexão antes do cursor e ler resultado assincronamente | Corrigido nesta auditoria de `Open()` para `OpenAsync()` |
| Token não enviado/expirado | `Empresa.Web/src/lib/api.ts` | Cliente Axios único, Bearer automático, fila durante refresh e redirecionamento ao login | Incorporado no código; faltavam critérios obrigatórios no template |
| Perfil/chave divergentes | PRDs e `GUIA_MENU.md` definem `chaveControle` como contrato central | Gate server-side, `ROUTE_MAP`, menu dinâmico e `MenuGuard` | A configuração efetiva usa `comprasFrutasPorEmpresas`; divergência corrigida por errata no PRD, sem alterar o gate |
| Menu/rota sem acesso | Fase 18 registra G-01 a G-03 e RN-12 | Policy `PaginaAcesso`, gate no service, `MenuGuard`, tela sem permissão e testes 403 | Implementado, mas nem todos os endpoints usam a policy parametrizada; vários usam gate próprio |
| Falha mascarada como menu/dados vazios | `PRD_MENU_LATERAL.md` D-03/D-09 | Propagar falha de banco como 500 e manter fallback apenas no shell | Incorporado no menu; precisa ser critério universal para stores de painéis |
| Contrato API/frontend divergente | Fase 18 G-03: `nome/icone` versus `tituloMenu/chaveControle` | DTOs tipados, services e tipos TypeScript alinhados | Incorporado nos módulos recentes; sem geração automática de contrato |
| Loading/erro/refresh de painel | Hooks Zustand e PRDs recentes | Estados `loading/error`, recarga, auto-refresh e testes Vitest | Parcial: existe por módulo, sem componente/padrão comum e sem teste E2E |

## Evidências de Aprendizado Incorporado

- `DbSession` é scoped e centraliza a connection string.
- `/api/health/database` distingue indisponibilidade do Oracle com 503.
- `ExceptionHandlingMiddleware` padroniza 400/403/404/409/422/500 e inclui `traceId`.
- `lib/api.ts` centraliza JWT, refresh e timeout de 30 segundos.
- `GUIA_MENU.md` documenta `chaveControle`, `ROUTE_MAP`, `MenuGuard`, policy e cache.
- `MenuServiceTests`, `PaginaAutorizacaoHandlerTests`, `AcessoRepositoryTests`, `AgricolaServiceTests` e testes Vitest cobrem parte dos incidentes anteriores.
- O PRD agrícola incluiu gates, contrato dinâmico, estados de carregamento, auto-refresh e testes de contrato.

## Lacunas Remanescentes

1. O Oracle de desenvolvimento continua inalcançável. Schema, packages, triggers e fidelidade das consultas não estão homologados com dados reais.
2. Não há suíte E2E automatizada cobrindo login → menu → autorização → carga do painel.
3. `MenuGuard` permite a rota quando o menu ainda não carregou; isto é aceitável somente porque o backend deve negar o dado. Um endpoint sem gate continua vulnerável.
4. Há documentação antiga divergente, especialmente `docs/PRD_FRONTEND_PAINEIS.md`, que cita MUI/React Query/React Router 7 em vez da stack vigente. Ela não deve ser usada como template.
5. A chave de permissão é string literal em vários services. Sem um catálogo canônico compartilhado, novas divergências ainda podem ocorrer.
6. O health check usa abertura síncrona e expõe `ex.Message`; merece refatoração própria, sem bloquear esta auditoria.
7. Os testes com `FakeDbConnection` validam SQL/contrato, mas não comportamento do ODP.NET, REF CURSOR ou tipos Oracle reais.

## Veredito

**O fluxo incorporou o aprendizado parcialmente e agora possui um padrão explícito, mas ainda não há garantia total.** A garantia depende de três controles: uso obrigatório do novo checklist, teste E2E de acesso/carregamento e homologação no Oracle real. Até a conexão ser restabelecida, features dependentes de banco devem ser registradas como concluídas com bloqueio de integração, nunca como integralmente homologadas.
