#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────
# setup-vps.sh — Script de inicialização da VPS Hostinger (Ubuntu)
# Execução: curl -fsSL <URL> | bash ou bash setup-vps.sh
# ─────────────────────────────────────────────────────────────────

set -e

echo "=== [1/6] Atualizando pacotes do sistema ==="
sudo apt-get update && sudo apt-get upgrade -y
sudo apt-get install -y curl wget git ufw certbot

echo "=== [2/6] Configurando Firewall (UFW) ==="
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

echo "=== [3/7] Configurando 2GB de Memória Swap (recomendado para 1 vCPU / 4GB) ==="
if [ ! -f /swapfile ]; then
    sudo fallocate -l 2G /swapfile || sudo dd if=/dev/zero of=/swapfile bs=1M count=2048
    sudo chmod 600 /swapfile
    sudo mkswap /swapfile
    sudo swapon /swapfile
    if ! grep -q "/swapfile" /etc/fstab; then
        echo "/swapfile none swap sw 0 0" | sudo tee -a /etc/fstab
    fi
    sudo sysctl vm.swappiness=10
    if ! grep -q "vm.swappiness" /etc/sysctl.conf; then
        echo "vm.swappiness=10" | sudo tee -a /etc/sysctl.conf
    fi
fi

echo "=== [4/7] Habilitando módulo PPP para FortiClient SSL VPN (openfortivpn) ==="
sudo modprobe ppp_generic
if ! grep -q "^ppp_generic$" /etc/modules; then
    echo "ppp_generic" | sudo tee -a /etc/modules
fi

echo "=== [5/7] Instalando Docker & Docker Compose ==="
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com | sh
    sudo usermod -aG docker "$USER"
fi

echo "=== [6/7] Criando estrutura de pastas em /opt/gestaonew ==="
sudo mkdir -p /opt/gestaonew/deploy/vpn
sudo mkdir -p /opt/gestaonew/nginx/conf.d
sudo mkdir -p /opt/gestaonew/nginx/ssl
sudo mkdir -p /opt/gestaonew/nginx/certbot-www
sudo mkdir -p /opt/gestaonew/logs/api
sudo mkdir -p /opt/gestaonew/logs/nginx
sudo chown -R "$USER":"$USER" /opt/gestaonew

echo "=== [7/7] Configuração concluída! ==="
echo ""
echo "Próximos passos manuais:"
echo "1. Copie a pasta deploy/vpn (Dockerfile do openfortivpn) para: /opt/gestaonew/deploy/vpn"
echo "2. Copie o docker-compose.prod.yml para /opt/gestaonew/"
echo "3. Crie /opt/gestaonew/.env a partir do deploy/.env.example e preencha"
echo "   as variáveis da VPN FortiClient (VPN_HOST, VPN_PORT, VPN_USERNAME, VPN_PASSWORD)"
echo "4. Inicie os serviços com: docker compose -f docker-compose.prod.yml up -d --build"
