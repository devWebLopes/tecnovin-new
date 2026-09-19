#!/bin/sh
# ─────────────────────────────────────────────
# entrypoint.sh — Gera a config do openfortivpn a partir das
# variáveis de ambiente e inicia o túnel FortiClient SSL VPN.
# A config (com a senha) fica apenas em memória/disco do container (chmod 600).
# ─────────────────────────────────────────────
set -eu

: "${VPN_HOST:?VPN_HOST nao definido (ex: vpn1.tecnovin.com.br)}"
: "${VPN_PORT:?VPN_PORT nao definido (ex: 10443)}"
: "${VPN_USERNAME:?VPN_USERNAME nao definido}"
: "${VPN_PASSWORD:?VPN_PASSWORD nao definido}"

CONFIG=/etc/openfortivpn/config
mkdir -p /etc/openfortivpn

{
    echo "host = ${VPN_HOST}"
    echo "port = ${VPN_PORT}"
    echo "username = ${VPN_USERNAME}"
    echo "password = ${VPN_PASSWORD}"
    # Realm da FortiGate (opcional)
    if [ -n "${VPN_REALM:-}" ]; then
        echo "realm = ${VPN_REALM}"
    fi
    # Fingerprint SHA-256 do certificado do gateway (opcional, recomendado)
    if [ -n "${VPN_TRUSTED_CERT:-}" ]; then
        echo "trusted-cert = ${VPN_TRUSTED_CERT}"
    fi
    # Aceita qualquer certificado — apenas se VPN_INSECURE_SSL=1 (ultimo recurso)
    if [ "${VPN_INSECURE_SSL:-0}" = "1" ]; then
        echo "insecure-ssl = 1"
    fi
    # Log do pppd para diagnostico (visivel em docker logs)
    echo "pppd-log = 1"
} > "$CONFIG"
chmod 600 "$CONFIG"

echo "[vpn] Conectando em ${VPN_HOST}:${VPN_PORT} (usuario: ${VPN_USERNAME})..."
exec openfortivpn -c "$CONFIG"
