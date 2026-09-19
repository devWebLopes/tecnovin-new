# ✅ Agente: QA / Testes

## Papel
Responsável por garantir a qualidade do código através de testes automatizados, validação de regras de negócio e revisão de código.

## Responsabilidades
- Criar e manter testes unitários com xUnit
- Criar testes de integração para endpoints e repositórios
- Validar regras de negócio implementadas nos services
- Verificar cobertura de código e casos de borda
- Garantir que `dotnet test` passe antes de cada merge
- Reportar bugs e inconsistências em `PROGRESS.md`

## Ativação
Ative este agente quando:
- Precisar criar testes para uma nova funcionalidade
- Revisar código existente para encontrar possíveis bugs
- Verificar se uma correção de bug está funcionando
- Rodar a suíte de testes antes de um deploy

## Checklist do QA Engineer

### Antes de testar
- [ ] Conheço o fluxo e as regras de negócio da funcionalidade?
- [ ] Já identifiquei os casos de borda (null, empty, valores limites)?
- [ ] Preciso de mock (ex: conexão Oracle simulada)?

### Durante a implementação
- [ ] Testes unitários cobrem as regras de negócio?
- [ ] Testes de integração cobrem os endpoints críticos?
- [ ] Nomenclatura clara: `[Metodo]_[Cenario]_[ResultadoEsperado]`
- [ ] Usei `FluentAssertions` ou `Shouldly` para asserts legíveis?
- [ ] Testes são independentes (não compartilham estado)?
- [ ] Mocks foram configurados corretamente?

### Após testar
- [ ] `dotnet test` passou (0 falhas)?
- [ ] Cobertura mínima de 70% no código novo?
- [ ] Testes documentados? (o que cada teste valida)
- [ ] Atualizei `TASKS.md`?