# 🚦 Skill: Workflow Enforcer (Governança Obrigatória)

**Descrição:** 
Esta é a skill primária de orquestração. Ela DEVE ser consultada antes de escrever ou modificar qualquer código no projeto GestaoNew.

**Regras de Execução Inquebráveis:**

1. **Leitura Obrigatória de Estado:**
   - O agente NUNCA deve iniciar uma tarefa sem ler o arquivo `TASKS.md` (para saber o que fazer) e o `PROGRESS.md` (para saber o que já foi tentado e falhou).
   
2. **Carregamento de Contexto sob Demanda:**
   - Para regras de código, leia sempre `docs/coding-standards.md` e `docs/naming-conventions.md`.
   - Para dúvidas estruturais, consulte `docs/architecture.md` e `docs/project-structure.md`.
   - Se a tarefa envolver banco de dados, é estritamente obrigatório aplicar as regras de `skills/oracle-best-practices.md` e `skills/dapper-orm.md`. Nenhuma query pode ser gerada sem validar essas skills.
   - Se envolver estrutura da aplicação, carregue `skills/clean-architecture-dotnet.md`.

3. **Validação e Fechamento:**
   - O código final deve respeitar as diretrizes de `docs/error-handling.md` e `docs/security.md`.
   - Antes de dar a tarefa como concluída, o agente deve rodar o build (`dotnet build`).
   - Se compilar com sucesso, o agente deve atualizar o `TASKS.md` (marcando com [x]) e o `PROGRESS.md` detalhando a entrega.

**Sinalizador de Violação:**
Se o usuário pedir para gerar código e você não tiver lido os arquivos de estado (TASKS/PROGRESS) na mesma sessão, você deve parar e pedir permissão para ler o contexto antes de prosseguir.