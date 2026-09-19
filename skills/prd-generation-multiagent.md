# 📝 Skill: Geração de PRDs — Multi-Agentes

## Sobre
Define o padrão de excelência para a criação de Product Requirements Documents (PRDs) no ecossistema GestaoNew. Esta skill garante que todo novo requisito seja projetado estritamente para sistemas multi-agentes e que o rastreamento de estado do projeto permaneça sincronizado.

## Configuração
Esta skill deve ser carregada pelo agente **Architect** ou pelo orquestrador principal (via Cursor, Claude Code, etc.) sempre que houver o planejamento de uma nova feature, módulo ou épico.

## Padrões de Implementação
Todo PRD gerado no projeto deve, obrigatoriamente, conter a seguinte estrutura em Markdown:
1. **Visão Geral e Objetivos:** Resumo do valor de negócio.
2. **Contexto Arquitetural:** Síntese baseada no `CLAUDE.md` e artefatos de `docs/`.
3. **Topologia Multi-Agente:** Quais agentes atuarão (ex: Architect, Backend Engineer, QA) e como colaboram.
4. **Casos de Uso e Fluxos:** Interação do usuário e comunicação inter-agentes.
5. **Mapeamento de Tarefas e Agentes:** Tabela associando `ID da Tarefa | Descrição | Agente Ideal`.
6. **Critérios de Aceite:** Definição de "Pronto" (DoD).

## Regras Obrigatórias
1. **Context Grounding:** O agente DEVE ler `CLAUDE.md` e varrer a pasta `docs/` antes de escrever qualquer linha do PRD.
2. **Sincronização de Tarefas:** O agente DEVE extrair as tarefas geradas no PRD e adicioná-las imediatamente ao arquivo `TASKS.md`.
3. **Sincronização de Sessão:** O agente DEVE registrar a criação do PRD e o próximo passo bloqueante no arquivo `PROGRESS.md`.
4. **Design Desacoplado:** A solução proposta DEVE utilizar a especialização dos agentes descritos no manifesto em vez de fluxos de desenvolvimento generalistas.

## Anti-Padrões (Proibidos)
- ❌ **Propor arquiteturas monolíticas:** Não decompor a execução entre os agentes especialistas disponíveis.
- ❌ **Gerar PRDs "órfãos":** Criar o documento sem atualizar a fila de execução no `TASKS.md` ou o estado no `PROGRESS.md`.
- ❌ **Ignorar o histórico:** Iniciar a concepção da arquitetura sem ler as restrições globais do `CLAUDE.md`.

## Checklist
- [ ] O `CLAUDE.md` e a pasta `docs/` foram lidos para absorver o contexto?
- [ ] O PRD gerado possui a seção "Mapeamento de Tarefas e Agentes"?
- [ ] As tarefas mapeadas foram copiadas para o `TASKS.md`?
- [ ] O registro da sessão foi atualizado no `PROGRESS.md`?