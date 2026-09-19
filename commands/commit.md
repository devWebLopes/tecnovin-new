# 💾 Command: Criar Commit

## Quando usar
Sempre que finalizar uma tarefa e precisar commitar as alterações no Git.

## Prompt para execução

```markdown
Você está preparando um commit para o projeto GestãoNew.

## Convenção de Mensagens de Commit

Use o formato: `tipo(escopo): descrição`

### Tipos
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `refactor`: Refatoração sem mudança de comportamento
- `test`: Adição ou modificação de testes
- `docs`: Documentação
- `chore`: Tarefas de manutenção (build, CI, config)
- `style`: Formatação, linting (sem mudança de código)

### Exemplos
- `feat(usuarios): criar CRUD de usuários`
- `fix(auth): corrigir validação de token expirado`
- `refactor(data): extrair DbSession para classe separada`
- `test(usuarios): adicionar testes do UsuarioRepository`
- `docs(readme): atualizar instruções de setup`

## Tarefas
- [ ] Verificar arquivos modificados (`git status`)
- [ ] Revisar se há arquivos temporários ou desnecessários
- [ ] Adicionar arquivos relevantes (`git add .` ou `git add <arquivos>`)
- [ ] Criar commit com mensagem seguindo a convenção
- [ ] Push (se aplicável)