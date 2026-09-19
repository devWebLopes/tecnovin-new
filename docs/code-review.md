# 🔍 Code Review — GestãoNew

## Objetivo

Este documento serve como **checklist** para revisões de código no projeto GestãoNew. Todo código deve ser revisado antes de ser mergeado ao branch principal.

---

## Checklist de Code Review

### 📐 Arquitetura e Design

- [ ] A solução compila sem warnings (`dotnet build`)
- [ ] O código segue a Clean Architecture (dependências corretas entre camadas)
- [ ] Não há dependências circulares entre projetos
- [ ] `Empresa.Data` não referencia `Empresa.Api` ou `Empresa.Worker`
- [ ] `Empresa.Util` não referencia nenhuma outra camada
- [ ] Arquivos estão nos diretórios corretos (Endpoints, Services, Models, etc.)
- [ ] Nenhum `Class1.cs` template esquecido

### ✍️ Padrões de Código

- [ ] Nomes de classes, métodos e propriedades em PascalCase (inglês)
- [ ] Nomes de parâmetros e variáveis locais em camelCase
- [ ] Comentários e documentação XML em português
- [ ] Namespaces usam file-scoped (`namespace X.Y;`)
- [ ] Nullable habilitado, com valores padrão (`= string.Empty`)
- [ ] Usings organizados (System → Third-party → Microsoft → Empresa)
- [ ] Código segue a formatação (4 espaços, chaves consistentes)

### 🔒 Segurança

- [ ] Nenhuma senha hardcoded no código
- [ ] Configurações sensíveis via User Secrets ou Environment Variables
- [ ] Senhas armazenadas com hash (BCrypt/PBKDF2) — nunca texto plano
- [ ] Todas as queries SQL usam parâmetros (bind variables)
- [ ] Nenhuma concatenação de strings SQL
- [ ] Nenhuma entidade `Model` exposta diretamente em endpoints (DTOs)

### ⚡ Async/Await

- [ ] Operações de I/O são async
- [ ] Nenhum `.Result`, `.Wait()` ou `.GetAwaiter().GetResult()`
- [ ] Métodos async têm sufixo `Async`
- [ ] Recursos não gerenciados usam `using` ou `await using`

### 🌐 API (Minimal API)

- [ ] Endpoints usam `IResult` padronizado (`Results.Ok`, `Results.NotFound`, etc.)
- [ ] Rotas seguem o padrão `/api/<dominio>` (plural)
- [ ] Endpoints validam a entrada antes de processar
- [ ] Handlers são privados estáticos no arquivo de endpoints
- [ ] Endpoints registrados via extension method no `Program.cs`
- [ ] Não há lógica de negócio diretamente nos endpoints

### 🗄️ Camada de Dados

- [ ] Queries SQL especificam colunas (nunca `SELECT *`)
- [ ] Parâmetros Oracle usam `:nome` (não `@nome`)
- [ ] `const string sql` para queries estáticas
- [ ] Repositórios têm interfaces e implementações separadas
- [ ] Exclusão lógica (coluna `ATIVO = 'N'`) em vez de `DELETE`
- [ ] Conexões não são compartilhadas entre requisições

### 🧪 Testes

- [ ] Testes unitários existem para Services e Utilitários (quando aplicável)
- [ ] Testes seguem o padrão Arrange-Act-Assert
- [ ] Nomes de teste descritivos: `[Unidade]_[Cenario]_[Resultado]`
- [ ] Mocks usam Moq (nenhuma dependência real)
- [ ] Testes são independentes entre si
- [ ] Cobertura mínima de 70% (Services) / 80% (Utilitários)

### 🔄 Injeção de Dependência

- [ ] Dependências injetadas via construtor
- [ ] Ciclo de vida apropriado (Scoped, Transient, Singleton)
- [ ] DbSession registrado como Scoped
- [ ] Nenhum Service Locator (`IServiceProvider.GetService`)
- [ ] Nenhum container IoC de terceiros

---

## Perguntas Norteadoras

Ao revisar, faça estas perguntas:

1. **O código resolve o problema certo?**
2. **Está fácil de entender e manter?**
3. **Existem testes para os cenários principais?**
4. **Os nomes são claros e descritivos?**
5. **Há duplicação que poderia ser eliminada?**
6. **O código é seguro contra SQL Injection e vazamento de dados?**
7. **As exceções são tratadas adequadamente?**
8. **O código segue as convenções do projeto?**

---

## Processo de Code Review

### Antes da Revisão (Autor)
```
1. Execute `dotnet build` e corrija warnings
2. Execute `dotnet test` e garanta que passam
3. Revise seu próprio código antes de solicitar review
4. Mantenha PRs pequenos e focados (máx 200-300 linhas)
5. Inclua descrição clara do que foi feito e por que
```

### Durante a Revisão (Revisor)
```
1. Entenda o contexto do PR
2. Siga o checklist acima
3. Comente sobre o código, não sobre a pessoa
4. Distinga entre "deve mudar" e "sugestão"
5. Aprove apenas quando estiver satisfeito
```

### Depois da Revisão (Autor)
```
1. Responda a todos os comentários
2. Faça as correções necessárias
3. Peça re-review se houver mudanças significativas
4. Merge após aprovação
```

---

## Comentários de Review

### Deve Mudar (🚫)
Use para problemas que impedem o merge:
```markdown
🚫 **Segurança**: Senha está sendo armazenada em texto plano.
Use `PasswordHasher.Hash()` antes de salvar.
```

### Sugestão (💡)
Use para melhorias que não bloqueiam o merge:
```markdown
💡 **Sugestão**: Considere usar `const string sql` em vez de
declarar a query como variável local para consistência.
```

### Dúvida (❓)
Use para esclarecimentos:
```markdown
❓ **Dúvida**: Este método parece não estar sendo usado em
lugar algum. Ele ainda é necessário?
```

---

## Anti-Padrões em Code Review

### ❌ O que EVITAR como Revisor
- Aprovar sem ler (`LGTM` sem revisar)
- Micro-gerenciamento (estilo pessoal vs padrões do projeto)
- Review monolítico (PR muito grande)
- Comentários vagos ("melhorar isso aqui")
- Revisar quando estiver cansado ou com pressa

### ❌ O que EVITAR como Autor
- PR com mais de 500 linhas
- Misturar reformatação com mudanças funcionais
- Não explicar o propósito do PR
- Ignorar feedback sem discutir
- Merge sem aprovação

---

## Ferramentas

### dotnet build (verificação de compilação)
```bash
dotnet build
# Sem warnings = ✅
```

### Análise Estática (SonarQube ou .NET Analyzers)
```bash
# O .NET SDK já inclui analisadores padrão
# Para análise mais profunda, configure SonarQube
```

### Verificação Automática de Padrões
```bash
# Verificar formatação
dotnet format --verify-no-changes

# Aplicar formatação automática
dotnet format
```

---

> **Use este checklist em todo code review.** Para tarefas pequenas, foque nos itens mais relevantes. Para tarefas grandes, revise item por item.