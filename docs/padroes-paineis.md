# Padrões de Implementação de Painéis

> Fonte operacional para novas features com acesso, Oracle e carregamento de dados.

## Objetivo

Evitar a repetição dos problemas encontrados nos painéis anteriores. Este checklist complementa `CLAUDE.md`, `docs/GUIA_MENU.md`, `docs/oracle-connection.md` e `commands/criar-feature.md`.

## Contrato de Acesso

- Definir uma única `chaveControle` antes de codificar.
- Usar exatamente a mesma chave no cadastro Oracle, `ROUTE_MAP`, gate do service, testes e documentação.
- Proteger o grupo de endpoints com `.RequireAuthorization()`.
- Aplicar gate server-side por perfil; `MenuGuard` é apenas proteção de UX.
- Tratar 401 como ausência/invalidez de autenticação, 403 como falta de permissão e 500 como falha interna/banco.
- Testar: sem token, token válido sem permissão, token válido com permissão e acesso direto pela URL.

## Contrato de Conexão Oracle

- Usar `DbSession` scoped e nunca compartilhar conexão entre requisições.
- Usar Dapper para SQL e os padrões Oracle documentados nas skills.
- Em `OracleCommand`/REF CURSOR, abrir a conexão com `OpenAsync()` antes da execução.
- Dispor command, reader e conexão conforme o escopo; não usar `.Result`, `.Wait()` ou `Open()` em I/O.
- Confirmar package, procedure, nomes, ordem e tipos dos parâmetros no Oracle real.
- Quando o Oracle estiver indisponível, criar teste de contrato e registrar o bloqueio. Teste fake não substitui homologação integrada.
- Verificar `GET /api/health/database` antes do aceite integrado.

## Contrato de API e Carregamento

- Definir DTO e exemplo JSON antes da tela; frontend e backend devem usar os mesmos nomes e tipos.
- Usar o cliente único `Empresa.Web/src/lib/api.ts`, que injeta JWT e trata refresh em 401.
- Toda tela deve ter estados explícitos de carregamento, sucesso, vazio e erro.
- Erros de timeout/500 devem produzir mensagem acionável e opção de recarga; não retornar lista vazia silenciosamente.
- Filtros e datas devem ser enviados no formato definido pelo contrato, preferencialmente ISO 8601.
- Auto-refresh deve ser cancelado no ciclo de vida da tela e não iniciar nova carga enquanto outra estiver ativa.
- Drill-downs devem ter seus contratos testados separadamente do painel principal.

## Evidências Mínimas de Aceite

- `dotnet build` e `dotnet test`.
- `npm run lint`, `npm run test` e `npm run build`.
- Testes de repositório/contrato, service/gate e store/hook de carregamento.
- Teste integrado: login → menu autorizado → rota → dados → drill-down, quando houver.
- Teste negativo: URL sem permissão → 403 no backend e tela sem permissão no frontend.
- Validação Oracle real ou bloqueio explícito em `TASKS.md` e `PROGRESS.md`.
