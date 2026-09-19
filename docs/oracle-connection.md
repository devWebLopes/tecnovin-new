# 🔌 Conexão Oracle — Guia Completo de Parâmetros

> **Driver:** Oracle.ManagedDataAccess.Core **23.26.300** (ODP.NET Managed)
> **Consumo:** `Empresa.Data/DbSession.cs` → `ConnectionStrings:Oracle`
> **Referência oficial:** Oracle Data Provider for .NET — Connection String Attributes

---

## 1. Conexão Atual do Projeto (Desenvolvimento)

Definida em `Empresa.Api/appsettings.Development.json` e `Empresa.Worker/appsettings.Development.json`:

| Parâmetro | Valor |
|-----------|-------|
| **Host** | `192.168.1.4` |
| **Porta** | `1526` (não-padrão; padrão Oracle é 1521) |
| **Service Name** | `TECNOVIN` |
| **User ID** | `treisbi_teste2` |
| **Password** | `treisbi_teste2` |

```
Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.4)(PORT=1526)))(CONNECT_DATA=(SERVICE_NAME=TECNOVIN)));User ID=treisbi_teste2;Password=treisbi_teste2;
```

> ⚠️ **Bloqueio conhecido (Fase 18):** este servidor está inalcançável a partir do ambiente atual (`ORA-50000: Connection request timed out`), pendente validação de VPN/rede/firewall. Ver `PROGRESS.md`.

---

## 2. Formatos de `Data Source`

O ODP.NET Managed aceita 3 formatos:

### 2.1 Connect Descriptor completo (formato usado no projeto)

```
Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=servidor)(PORTA=1521)))(CONNECT_DATA=(SERVICE_NAME=nome_servico)))
```

### 2.2 Easy Connect (forma curta)

```
Data Source=servidor:1521/nome_servico
```

### 2.3 Alias TNS (requer `tnsnames.ora`)

```
Data Source=MEU_ALIAS;TNS_Admin=C:\oracle\network\admin
```

---

## 3. Parâmetros da Connection String (nível superior)

### 3.1 Autenticação

| Parâmetro | Descrição | Default |
|-----------|-----------|---------|
| `User Id` | Usuário do schema Oracle. Use `User Id=/` para autenticação de sistema operacional | — |
| `Password` | Senha do usuário | — |
| `DBA Privilege` | Privilégio administrativo da sessão: `SYSDBA`, `SYSOPER`, `SYSASM`, `SYSBACKUP`, `SYSDG`, `SYSKM`, `SYSRAC` | *(vazio)* |
| `Proxy User Id` | Usuário para conexão proxy (impersonation) | — |
| `Proxy Password` | Senha do usuário proxy | — |
| `Persist Security Info` | Se `false`, remove a senha da connection string após abrir a conexão (recomendado) | `false` |

### 3.2 Pooling de Conexões

| Parâmetro | Descrição | Default | Recomendado (skill `oracle-best-practices.md`) |
|-----------|-----------|---------|------------------------------------------------|
| `Pooling` | Habilita pool de conexões | `true` | `true` |
| `Min Pool Size` | Conexões mínimas mantidas abertas (mesmo ociosas) | `0` | `2` |
| `Max Pool Size` | Limite máximo de conexões no pool (evita estouro no banco) | `100` | `20` |
| `Connection Lifetime` | Tempo de vida (segundos) antes da conexão ser reciclada | `0` (sem limite) | `300` (5 min) |
| `Connection Timeout` | Espera (segundos) por conexão do pool antes de falhar | `15` | `15` |
| `Incr Pool Size` | Quantas conexões criar por vez quando o pool cresce | `5` | default |
| `Decr Pool Size` | Quantas conexões liberar por vez quando o pool reduz | `1` | default |
| `Validate Connection` | Valida a conexão ao retirá-la do pool (custo extra por operação) | `false` | `false` |

### 3.3 Comportamento / Performance

| Parâmetro | Descrição | Default |
|-----------|-----------|---------|
| `Enlist` | Inscrição em transação distribuída: `true`, `false` ou `dynamic` | `true` |
| `Statement Cache Size` | Nº de statements cacheados por conexão (0 desabilita) | `0` |
| `Statement Cache Purge` | Limpa o cache ao devolver a conexão ao pool | `false` |
| `Self Tuning` | Auto-ajuste interno do cache de statements do driver | `true` |
| `Metadata Pooling` | Cache de metadados entre conexões do pool | `true` |
| `Application Continuity` | Reprodução transparente de transações após failover (requer serviço configurado no banco) | `false` |

### 3.4 Alta Disponibilidade (RAC / Data Guard)

| Parâmetro | Descrição | Default |
|-----------|-----------|---------|
| `HA Events` | Recebe eventos FAN (Fast Application Notification) via ONS — drena conexões de nós que caíram | `false` |
| `Load Balancing` | Balanceamento em tempo de execução baseado em carga (requer `HA Events=true`) | `false` |

### 3.5 Segurança / TLS

| Parâmetro | Descrição | Default |
|-----------|-----------|---------|
| `TNS_Admin` | Diretório de `tnsnames.ora`, `sqlnet.ora` e/ou wallet (substitui configuração de registro/ambiente) | — |
| `Wallet_Location` | Diretório do Oracle Wallet (certificados) para conexões `TCPS` | — |

---

## 4. Parâmetros do Connect Descriptor (dentro de `DESCRIPTION`)

### 4.1 Endereço (`ADDRESS`)

| Parâmetro | Descrição |
|-----------|-----------|
| `PROTOCOL` | `TCP` (texto claro) ou `TCPS` (TLS — exige wallet/certificado) |
| `HOST` | Nome DNS ou IP do servidor Oracle |
| `PORT` | Porta do listener (padrão `1521`; o projeto usa `1526`) |

### 4.2 Identificação do banco (`CONNECT_DATA`)

| Parâmetro | Descrição |
|-----------|-----------|
| `SERVICE_NAME` | Nome do serviço registrado no listener (**usado no projeto**) — recomendado |
| `SID` | Identificador da instância (legado; prefira `SERVICE_NAME`) |
| `INSTANCE_NAME` | Instância específica em ambientes RAC |
| `SERVER` | `DEDICATED` (default) ou `SHARED` (shared server) |

### 4.3 Timeouts e retentativas (nível `DESCRIPTION`)

| Parâmetro | Descrição | Default |
|-----------|-----------|---------|
| `CONNECT_TIMEOUT` | Segundos aguardando resposta de cada endereço | `60` |
| `TRANSPORT_CONNECT_TIMEOUT` | Segundos para estabelecer a conexão TCP | `60` |
| `RETRY_COUNT` | Vezes que a lista de endereços é percorrida após falha | `0` |
| `RETRY_DELAY` | Segundos entre retentativas | `0` |
| `SDU` | Session Data Unit (bytes) — buffers maiores para grandes volumes | `8192` |
| `ENABLE` | `BROKEN` — ativa sondas keepalive TCP para detectar conexões mortas | — |

### 4.4 Failover / Balanceamento (nível `ADDRESS_LIST`)

| Parâmetro | Descrição |
|-----------|-----------|
| `FAILOVER` | `ON` — tenta o próximo endereço da lista em caso de falha |
| `LOAD_BALANCE` | `ON` — distribui conexões aleatoriamente entre os endereços |

---

## 5. Onde Configurar no Projeto (ordem de precedência)

O .NET Configuration resolve na ordem abaixo (a última vence):

1. `appsettings.json` — **`"Oracle": ""` vazio (proposital — sem segredos commitados)**
2. `appsettings.{Environment}.json` — desenvolvimento usa `appsettings.Development.json`
3. **User Secrets** (desenvolvimento local, fora do repositório):
   ```bash
   dotnet user-secrets init --project Empresa.Api
   dotnet user-secrets set "ConnectionStrings:Oracle" "Data Source=...;User Id=...;Password=..." --project Empresa.Api
   ```
4. **Variável de ambiente** (produção / Docker):
   ```bash
   # Windows
   setx ConnectionStrings__Oracle "Data Source=...;User Id=...;Password=..."
   # Linux / Docker
   export ConnectionStrings__Oracle="Data Source=...;User Id=...;Password=..."
   ```
   No `docker-compose.yml`, a variável `ORACLE_CONNECTION_STRING` (do `.env`, **nunca commitado**) é mapeada para `ConnectionStrings__Oracle`.

---

## 6. Exemplo Recomendado (produção, com pooling)

```
Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.4)(PORT=1526)))(CONNECT_DATA=(SERVICE_NAME=TECNOVIN)));User ID=usuario;Password=senha;Pooling=true;Min Pool Size=2;Max Pool Size=20;Connection Lifetime=300;Connection Timeout=15;
```

---

## 7. Como Testar a Conexão

| Método | Comando / Ação |
|--------|----------------|
| Health check da API | `GET http://localhost:5000/api/health/database` |
| Diagnóstico em teste | `dotnet test --filter DatabaseDiagnosticTests` |
| SQLcl / SQL*Plus | `sql treisbi_teste2/treisbi_teste2@192.168.1.4:1526/TECNOVIN` |
| Listener | `tnsping 192.168.1.4:1526` (requer client Oracle) |

---

## 8. Troubleshooting

| Erro | Causa provável | Ação |
|------|----------------|------|
| `ORA-12170` / `ORA-12535` | Timeout de rede — host/porta inalcançável, firewall ou VPN | Testar `Test-NetConnection 192.168.1.4 -Port 1526` |
| `ORA-50000` (timeout de request) | Conexão aceita mas sem resposta — observado neste projeto | Validar VPN/rede com a infraestrutura |
| `ORA-12514` | Listener não conhece o `SERVICE_NAME` | Confirmar serviço com o DBA (`lsnrctl services`) |
| `ORA-12541` | Nenhum listener na porta | Confirmar porta (`1526` vs `1521`) |
| `ORA-01017` | Usuário/senha inválidos | Verificar credenciais e expiração da conta |
| `ORA-12154` | Alias TNS não resolvido | Conferir `TNS_Admin` e `tnsnames.ora` |
| `ORA-28000` / `ORA-28001` | Conta bloqueada / senha expirada | Solicitar desbloqueio/reset ao DBA |

---

## 9. Guardrails (CLAUDE.md)

- **Nunca** commite senhas — `appsettings.json` mantém `"Oracle": ""` propositalmente
- **Nunca** commite `.env`, `appsettings.*.local` ou `secrets.json`
- Conexão **Scoped** (uma por requisição) via `DbSession` — nunca compartilhe `DbConnection` entre requisições
- Sempre `using`/`await using` para disposição da conexão
- Dapper gerencia `Open()`/`Close()` automaticamente; o pool devolve a conexão ao `Dispose()`
