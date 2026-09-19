# 🚀 Manual de Deploy em Produção — GestãoNew

> **Topologia:** VPS Hostinger (Ubuntu) + Docker Compose + **FortiClient SSL VPN (sidecar openfortivpn)** + API .NET 9 + Web React + Nginx Gateway com SSL Let's Encrypt
> **Arquivos de referência:** `docker-compose.prod.yml` · `Dockerfile` · `Empresa.Web/Dockerfile` · `deploy/vpn/Dockerfile` · `deploy/vpn/entrypoint.sh` · `deploy/setup-vps.sh` · `deploy/.env.example` · `deploy/nginx/conf.d/app.conf` · `.github/workflows/deploy.yml`

---

## 1. Arquitetura de Produção

```
                        ┌──────────────────────── VPS Hostinger ────────────────────────┐
                        │                                                               │
 Internet ──80/443──►   │  gateway (nginx :80/:443)                                     │
                        │     │                                                         │
                        │     ├── /      ──► web (React SPA · nginx interno :80)        │
                        │     │                                                         │
                        │     └── /api/  ──► api (.NET 9 :8080)                         │
                        │                        │ network_mode: "service:vpn"          │
                        │                        ▼ (compartilha a rede do container VPN)│
                        │                 vpn (openfortivpn — FortiClient SSL VPN)      │
                        └──────────────────────────┼────────────────────────────────────┘
                                                   │ túnel FortiClient SSL VPN (ppp0)
                                                   ▼
                              Oracle 192.168.1.4:1526 · SERVICE_NAME=TECNOVIN
                              (rede interna da empresa — inacessível sem VPN)
```

### ⚠️ Ponto crítico: a VPN é obrigatória

O Oracle **não é exposto na internet** — ele vive na rede interna (`192.168.1.4:1526`). A API só alcança o banco porque **compartilha o namespace de rede do container VPN** (`network_mode: "service:vpn"` no compose). Consequências:

- Se a VPN não conectar, a API sobe, mas **toda consulta ao banco falha** (`ORA-12170`/timeout).
- O healthcheck do container VPN (`ping 192.168.1.4`) precisa estar `healthy` **antes** da API iniciar — o compose já garante isso via `depends_on: service_healthy`.
- O Nginx faz proxy para a API pelo nome `vpn` (a API "vive" na rede do container VPN): `proxy_pass http://vpn:8080/api/`.

---

## 2. Pré-requisitos (checklist)

Antes de começar, tenha em mãos:

- [ ] VPS Ubuntu 22.04/24.04 (Hostinger) com acesso SSH e usuário com `sudo`
- [ ] **Domínio com registro A apontado para o IP da VPS** (ex.: `gestao.tecnovin.com.br`) — necessário para o SSL
- [ ] **Credenciais da VPN FortiClient** (gateway `vpn1.tecnovin.com.br:10443`, usuário e senha) — fornecidas pela equipe de infraestrutura
- [ ] **Credenciais Oracle de produção** (usuário/senha do schema — ex.: `treisbi`) — fornecidas pelo DBA
- [ ] Repositório no GitHub com Actions habilitado (para CI/CD via GHCR) **ou** Docker na máquina local para build/push manual

---

## 3. Passo 1 — Preparar a VPS

Acesse a VPS e execute o script de bootstrap do projeto:

```bash
ssh usuario@IP_DA_VPS

# Opção A: repositório clonado na VPS
bash deploy/setup-vps.sh

# Opção B: direto do GitHub
curl -fsSL https://raw.githubusercontent.com/<org-ou-usuario>/gestaonew/main/deploy/setup-vps.sh | bash
```

O script executa automaticamente:

| Etapa | O que faz |
|-------|-----------|
| Pacotes | `apt update/upgrade`, instala `curl`, `wget`, `git`, `ufw`, `certbot` |
| Firewall (UFW) | Libera apenas `22` (SSH), `80` e `443` |
| Swap | Cria 2 GB de swap (recomendado para VPS de 1 vCPU/4 GB) |
| **Módulo PPP** | `modprobe ppp_generic` + persistência em `/etc/modules` — **obrigatório para o openfortivpn** (o túnel FortiClient usa `pppd`, interface `ppp0`) |
| Docker | Instala Docker Engine + Compose plugin, adiciona o usuário ao grupo `docker` |
| Estrutura | Cria `/opt/gestaonew/{deploy/vpn,nginx/{conf.d,ssl,certbot-www},logs/{api,nginx}}` |

Após o script, faça **logout e login novamente** (ou `newgrp docker`) para usar `docker` sem `sudo`.

---

## 4. Passo 2 — Configurar a VPN FortiClient ⚠️ (etapa crítica)

> Sem este passo concluído e validado, **não prossiga** — a API não alcançará o Oracle.

A VPN é um **FortiClient SSL VPN** conectado pelo [`openfortivpn`](https://github.com/adrienverge/openfortivpn) (cliente open-source compatível com FortiGate), empacotado como sidecar Docker (`deploy/vpn/Dockerfile` — Alpine + `openfortivpn` + `ppp`). Diferente do OpenVPN, **não há arquivo de perfil** — toda a configuração vai no `.env`.

### 4.1 Copiar o Dockerfile da VPN para a VPS

A imagem da VPN é construída **na própria VPS** (o compose usa `build: ./deploy/vpn`). Na sua máquina local:

```bash
scp -r deploy/vpn usuario@IP_DA_VPS:/opt/gestaonew/deploy/vpn
```

### 4.2 Configurar as credenciais da VPN no `.env`

Edite `/opt/gestaonew/.env` (criado a partir do `deploy/.env.example` — ver Passo 3) e preencha o bloco da VPN:

```env
VPN_HOST=vpn1.tecnovin.com.br
VPN_PORT=10443
VPN_USERNAME=tecnovin
VPN_PASSWORD=SENHA_REAL_DA_VPN
```

Opções adicionais (raramente necessárias):

| Variável | Quando usar |
|----------|-------------|
| `VPN_REALM` | Se a FortiGate exigir um realm específico de autenticação |
| `VPN_TRUSTED_CERT` | **Recomendado** se o gateway usar certificado autoassinado — fingerprint SHA-256 do certificado (veja como obter abaixo) |
| `VPN_INSECURE_SSL=1` | Último recurso: aceita qualquer certificado. **Evite em produção** |

Para descobrir o fingerprint SHA-256 do certificado do gateway:

```bash
openssl s_client -connect vpn1.tecnovin.com.br:10443 </dev/null 2>/dev/null | openssl x509 -fingerprint -sha256 -noout
# → sha256 Fingerprint=AA:BB:CC:...
# Copie o valor (com ou sem os dois-pontos) para VPN_TRUSTED_CERT
```

> ⚠️ Se o certificado do gateway for autoassinado e nenhuma das duas variáveis estiver configurada, o `openfortivpn` **aborta a conexão** com erro de verificação de certificado — o log mostrará o fingerprint esperado, que pode ser copiado para `VPN_TRUSTED_CERT`.

### 4.3 Testar o túnel ISOLADAMENTE

Suba **somente** o serviço VPN e valide antes de subir o restante:

```bash
cd /opt/gestaonew
docker compose -f docker-compose.prod.yml up -d --build vpn
docker logs -f gestaonew-vpn
```

Aguarde a sequência **`INFO: Connected to gateway.`** / **`INFO: Tunnel is up and running.`** (Ctrl+C para sair do log — o container continua rodando). Em seguida:

```bash
# 1. A interface do túnel existe?
docker exec gestaonew-vpn ip addr show ppp0

# 2. Existe rota para a rede do Oracle via túnel?
docker exec gestaonew-vpn ip route
# → deve aparecer algo como: 192.168.1.0/24 dev ppp0 ...

# 3. O servidor Oracle responde?
docker exec gestaonew-vpn ping -c 3 192.168.1.4

# 4. A porta do listener Oracle está acessível?
docker exec gestaonew-vpn sh -c "timeout 5 sh -c 'echo > /dev/tcp/192.168.1.4/1526' && echo PORTA_OK"
```

✅ **Critério de aceite:** os 4 testes passam. Se o ping falhar ou não houver rota para `192.168.1.0/24`, **pare aqui** e acione a infraestrutura (provavelmente a política SSL VPN da FortiGate não está liberando/empurrando a rota da rede interna para o usuário `tecnovin`). Subir a API sem túnel funcional é perda de tempo.

---

## 5. Passo 3 — Configurar as variáveis de ambiente (`.env`)

```bash
cd /opt/gestaonew
nano .env
```

Baseie-se em `deploy/.env.example` (o bloco `VPN_*` do FortiClient já deve estar preenchido — ver Passo 2):

```env
# Oracle Database (produção — credenciais fornecidas pelo DBA)
ORACLE_CONNECTION_STRING=Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.4)(PORT=1526)))(CONNECT_DATA=(SERVICE_NAME=TECNOVIN)));User ID=treisbi;Password=SENHA_REAL_AQUI;Pooling=true;Min Pool Size=2;Max Pool Size=20;Connection Lifetime=300;Connection Timeout=15;

# JWT — gere uma chave NOVA e única (não reuse a do exemplo)
JWT_SECRET=SUBSTITUA_PELA_SAIDA_DE_openssl_rand_-hex_32
JWT_EXPIRY_MINUTES=60
JWT_REFRESH_DAYS=7

# Imagens Docker
API_IMAGE=ghcr.io/<org-ou-usuario>/gestaonew/api:latest
WEB_IMAGE=ghcr.io/<org-ou-usuario>/gestaonew/web:latest
```

Gerar o `JWT_SECRET` forte:

```bash
openssl rand -hex 32
```

Proteger o arquivo:

```bash
chmod 600 .env
```

> ⚠️ **Guardrails (CLAUDE.md):** o `.env` **nunca** deve ser commitado. Ele já está coberto pelo `.gitignore` da raiz.

---

## 6. Passo 4 — Copiar o compose e a configuração do Nginx

Na sua máquina local:

```bash
scp docker-compose.prod.yml usuario@IP_DA_VPS:/opt/gestaonew/
scp deploy/nginx/conf.d/app.conf usuario@IP_DA_VPS:/opt/gestaonew/nginx/conf.d/app.conf
```

Na VPS, edite o domínio real no gateway:

```bash
nano /opt/gestaonew/nginx/conf.d/app.conf
# Troque:  server_name _;
# Por:     server_name gestao.tecnovin.com.br;
```

> **Observação sobre DNS:** o frontend React chama a API por **caminho relativo** (`/api/v1` — ver `Empresa.Web/src/lib/api.ts`), então **um único domínio** resolve. Não é preciso subdomínio `api.*` separado.

---

## 7. Passo 5 — Publicar as imagens Docker

Escolha **uma** das três opções:

### Opção A — CI/CD GitHub Actions (recomendado)

O workflow `.github/workflows/deploy.yml` já faz tudo: build → testes → publica `ghcr.io/<org>/gestaonew/api:latest` e `web:latest` → deploy via SSH na VPS.

Configure os secrets em **Settings → Secrets and variables → Actions** do repositório:

| Secret | Conteúdo |
|--------|----------|
| `HOSTINGER_SSH_IP` | IP público da VPS |
| `HOSTINGER_SSH_USER` | Usuário SSH |
| `HOSTINGER_SSH_KEY` | Chave privada SSH (conteúdo completo) |
| `HOSTINGER_SSH_PORT` | Porta SSH (opcional, default 22) |

Depois, basta `git push` na branch `main`.

> **GHCR privado:** se os pacotes forem privados, a VPS precisa autenticar no registry antes do `pull`:
> ```bash
> echo SEU_GITHUB_PAT | docker login ghcr.io -u SEU_USUARIO_GITHUB --password-stdin
> ```
> (PAT com escopo `read:packages`). Alternativa: marcar os pacotes como públicos no GitHub.

### Opção B — Build e push manual (da sua máquina)

```bash
# Na raiz do projeto, logado no GHCR (docker login ghcr.io)
docker build -t ghcr.io/<org>/gestaonew/api:latest .
docker build -t ghcr.io/<org>/gestaonew/web:latest ./Empresa.Web
docker push ghcr.io/<org>/gestaonew/api:latest
docker push ghcr.io/<org>/gestaonew/web:latest
```

### Opção C — Build na própria VPS (sem registry)

```bash
git clone https://github.com/<org>/gestaonew.git /opt/gestaonew/src
cd /opt/gestaonew/src
docker build -t gestaonew/api:latest .
docker build -t gestaonew/web:latest ./Empresa.Web
```

E ajuste o `.env`:

```env
API_IMAGE=gestaonew/api:latest
WEB_IMAGE=gestaonew/web:latest
```

---

## 8. Passo 6 — SSL: certificado temporário e subida da stack

O gateway Nginx **só inicia se os arquivos de certificado existirem**. Como o certbot ainda não emitiu o certificado real (ele precisa do gateway no ar para o desafio ACME), faça o bootstrap com um certificado autoassinado:

```bash
mkdir -p /opt/gestaonew/nginx/ssl/live/gestao
openssl req -x509 -nodes -newkey rsa:2048 -days 2 \
  -keyout /opt/gestaonew/nginx/ssl/live/gestao/privkey.pem \
  -out /opt/gestaonew/nginx/ssl/live/gestao/fullchain.pem \
  -subj "/CN=gestao.tecnovin.com.br"
```

Agora suba a stack completa:

```bash
cd /opt/gestaonew
docker compose -f docker-compose.prod.yml pull        # apenas se usar registry (Opções A/B)
docker compose -f docker-compose.prod.yml up -d --build   # --build: constrói a imagem da VPN (deploy/vpn) na 1ª subida
docker compose -f docker-compose.prod.yml ps
```

Ordem de subida esperada: `vpn` (build local + aguarda ficar `healthy`) → `api` → `web` → `gateway`.

---

## 9. Passo 7 — Emitir o certificado SSL real (Let's Encrypt)

Com o gateway no ar respondendo na porta 80 (o `app.conf` já serve o desafio ACME em `/.well-known/acme-challenge/`):

```bash
sudo certbot certonly --webroot -w /opt/gestaonew/nginx/certbot-www \
  -d gestao.tecnovin.com.br \
  --email voce@tecnovin.com.br --agree-tos --no-eff-email
```

Substitua o certificado temporário pelo real e recarregue o Nginx:

```bash
sudo cp /etc/letsencrypt/live/gestao.tecnovin.com.br/fullchain.pem /opt/gestaonew/nginx/ssl/live/gestao/fullchain.pem
sudo cp /etc/letsencrypt/live/gestao.tecnovin.com.br/privkey.pem   /opt/gestaonew/nginx/ssl/live/gestao/privkey.pem
docker exec gestaonew-gateway nginx -s reload
```

### Renovação automática (cron)

Crie o script de pós-renovação:

```bash
sudo tee /opt/gestaonew/renew-cert.sh > /dev/null <<'EOF'
#!/usr/bin/env bash
set -e
cp /etc/letsencrypt/live/gestao.tecnovin.com.br/fullchain.pem /opt/gestaonew/nginx/ssl/live/gestao/fullchain.pem
cp /etc/letsencrypt/live/gestao.tecnovin.com.br/privkey.pem   /opt/gestaonew/nginx/ssl/live/gestao/privkey.pem
docker exec gestaonew-gateway nginx -s reload
EOF
sudo chmod +x /opt/gestaonew/renew-cert.sh
```

Agende a renovação:

```bash
echo "0 3 * * 1 root certbot renew --quiet --deploy-hook /opt/gestaonew/renew-cert.sh" | sudo tee /etc/cron.d/gestaonew-certbot
```

---

## 10. Passo 8 — Validação pós-deploy

Execute **na ordem** — cada item valida uma camada:

```bash
# 1. Containers no ar e VPN saudável
docker compose -f docker-compose.prod.yml ps
# → gestaonew-vpn deve constar como "healthy"; os demais "running"

# 2. Túnel VPN alcançando o Oracle
docker exec gestaonew-vpn ping -c 2 192.168.1.4

# 3. Health check geral da API (através do gateway, já com SSL)
curl https://gestao.tecnovin.com.br/api/health

# 4. Health check do Oracle — A PROVA de que a VPN está cumprindo seu papel
curl https://gestao.tecnovin.com.br/api/health/database

# 5. SPA respondendo
curl -I https://gestao.tecnovin.com.br/
# → HTTP/2 200

# 6. Logs
docker logs gestaonew-vpn --tail 30
docker logs gestaonew-api --tail 50
docker logs gestaonew-gateway --tail 30
ls /opt/gestaonew/logs/api/        # logs Serilog da API
```

**Teste funcional final:** abra `https://gestao.tecnovin.com.br` no navegador, faça login com um usuário real do sistema e abra um painel que consulte o Oracle (ex.: Compras ou Financeiro). Dados carregando = deploy validado de ponta a ponta (VPN → Oracle → API → SPA).

---

## 11. Atualizações futuras (redeploy)

### Via CI/CD (Opção A)

`git push` na `main` — o workflow publica as imagens e recria `api` e `web` na VPS automaticamente.

### Manual

```bash
cd /opt/gestaonew
docker compose -f docker-compose.prod.yml pull api web
docker compose -f docker-compose.prod.yml up -d --remove-orphans api web
docker image prune -f
```

> A VPN e o gateway **não** são recriados no redeploy — o túnel permanece de pé.

### Rollback

Fixe a tag da versão anterior no `.env` (as imagens também são tagueadas por SHA do commit: `ghcr.io/<org>/gestaonew/api:<sha>`):

```env
API_IMAGE=ghcr.io/<org>/gestaonew/api:<sha-anterior>
WEB_IMAGE=ghcr.io/<org>/gestaonew/web:<sha-anterior>
```

```bash
docker compose -f docker-compose.prod.yml up -d api web
```

---

## 12. Troubleshooting

| Sintoma | Causa provável | Ação |
|---------|----------------|------|
| `gestaonew-api` não inicia / reinicia | VPN não ficou `healthy` | `docker logs gestaonew-vpn` — validar `VPN_HOST`/`VPN_PORT`/`VPN_USERNAME`/`VPN_PASSWORD` no `.env` |
| `ORA-12170` / `ORA-12535` / timeout de conexão | Túnel caído ou **sem rota** para `192.168.1.0/24` | `docker exec gestaonew-vpn ip route`; pedir à infra que a política SSL VPN da FortiGate libere a rede `192.168.1.0/24` para o usuário |
| `ORA-12514` | `SERVICE_NAME` desconhecido pelo listener | Confirmar `TECNOVIN` com o DBA |
| `ORA-12541` | Nenhum listener na porta | Confirmar porta `1526` (não é a padrão `1521`) |
| `ORA-01017` | Usuário/senha Oracle inválidos | Revisar `ORACLE_CONNECTION_STRING` no `.env` |
| `ORA-28000` / `ORA-28001` | Conta bloqueada / senha expirada | Solicitar desbloqueio/reset ao DBA |
| Gateway não sobe (`no such file` em `ssl_certificate`) | Certificado ausente | Refazer o bootstrap do Passo 6 (cert temporário) |
| `502 Bad Gateway` em `/api/` | API fora do ar ou VPN não healthy | `docker compose ps` + `docker logs gestaonew-api` |
| Certbot falha no desafio ACME | DNS não aponta para a VPS ou porta 80 bloqueada | Conferir registro A e `sudo ufw status` |
| VPN conecta mas não pinga o Oracle | FortiGate não empurra/libera a rota da rede interna | Acionar infraestrutura: a política SSL VPN do usuário precisa dar acesso a `192.168.1.0/24` |
| `ERROR: SSL error` / falha de verificação de certificado | Certificado do gateway autoassinado | Configurar `VPN_TRUSTED_CERT` com o fingerprint mostrado no log (ou, como último recurso, `VPN_INSECURE_SSL=1`) |
| `ERROR: read: Input/output error` ou desconexões frequentes | Sessão derrubada pela FortiGate (timeout de sessão/idle) | Verificar timeouts na FortiGate; o `restart: always` do compose reconecta automaticamente |
| `Tunnel is up and running` não aparece | Credenciais VPN erradas / host ou porta incorretos / realm exigido | Conferir `VPN_HOST`, `VPN_PORT` (10443), `VPN_USERNAME`, `VPN_PASSWORD` e, se houver, `VPN_REALM` |
| Container VPN morre com `permission denied` em `/dev/ppp` | Módulo `ppp_generic` não carregado no host | `sudo modprobe ppp_generic` (o `setup-vps.sh` já persiste em `/etc/modules`) |

Comandos úteis de diagnóstico:

```bash
docker exec gestaonew-vpn ip route                 # rotas via ppp0
docker exec gestaonew-vpn ip addr show ppp0        # IP do túnel
docker compose -f docker-compose.prod.yml logs -f  # log agregado de todos os serviços
```

---

## 13. Checklist final de go-live

- [ ] VPN conectada (`Tunnel is up and running` no log do `gestaonew-vpn`)
- [ ] `ping 192.168.1.4` OK **de dentro** do container `gestaonew-vpn`
- [ ] `GET /api/health/database` retornando saudável **via HTTPS público**
- [ ] Cadeado SSL válido no domínio (sem aviso de certificado)
- [ ] Login real + painel com dados do Oracle carregando no navegador
- [ ] Renovação de certificado agendada no cron (`/etc/cron.d/gestaonew-certbot`)
- [ ] `.env` (com `VPN_PASSWORD` e `ORACLE_CONNECTION_STRING`) com `chmod 600` e **fora** do Git
- [ ] `JWT_SECRET` de produção único e forte (não é o do `.env.example`)

---

## 14. Observações

- **Worker:** o `Empresa.Worker` (background service) **não** faz parte da topologia de produção atual do `docker-compose.prod.yml`. Se for necessário em produção, adicione um serviço `worker` ao compose com `network_mode: "service:vpn"` (mesmo padrão da API) e as mesmas variáveis de conexão.
- **Timezone:** todos os containers já rodam com `TZ=America/Sao_Paulo`.
- **Logs:** a API grava logs Serilog em `/opt/gestaonew/logs/api/` (volume mapeado); o Nginx em `/opt/gestaonew/logs/nginx/`.
- **Conexão Oracle:** detalhes completos de parâmetros, pooling e troubleshooting em `docs/oracle-connection.md`.
