# 🐛 Command: Corrigir Bug

## Quando usar
Sempre que um bug for reportado no sistema — seja em produção, staging ou desenvolvimento.

## Prompt para execução

```markdown
Você é um Engenheiro de Software responsável por corrigir um bug no projeto GestãoNew (.NET 9, Dapper, Oracle).

## Contexto do Bug
- **Descrição:** [O que acontece, qual o comportamento esperado vs real]
- **Passos para reproduzir:** [Como reproduzir o bug]
- **Impacto:** [Crítico, Alto, Médio, Baixo]
- **Onde ocorre:** [Endpoint, service, query, etc.]

## Tarefas

### 1. Diagnóstico
- [ ] Reproduzir o bug (se possível)
- [ ] Verificar logs de erro
- [ ] Identificar a causa raiz (não apenas o sintoma)
- [ ] Verificar se já existe tentativa anterior que falhou em `PROGRESS.md`

### 2. Correção
- [ ] Implementar a correção na camada apropriada
- [ ] Garantir que a correção não quebre contratos arquiteturais
- [ ] Se for bug de banco/query, consultar `agents/database-engineer.md`
- [ ] Se for bug de lógica, consultar `agents/backend-engineer.md`

### 3. Prevenção
- [ ] Adicionar teste que cobre o cenário do bug (para não regredir)
- [ ] Verificar se há outros locais com o mesmo padrão problemático

### 4. Validação
- [ ] `dotnet build` sem erros
- [ ] Teste novo passando
- [ ] Bug não reproduz mais
- [ ] Atualizar `TASKS.md`
- [ ] Atualizar `PROGRESS.md` — registrar a falha corrigida

## Regras
- Nunca corrigir apenas o sintoma — sempre encontrar a causa raiz
- Sempre adicionar teste de regressão
- Consultar `PROGRESS.md` para ver se a mesma abordagem já falhou antes