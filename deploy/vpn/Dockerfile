# ─────────────────────────────────────────────
# VPN Sidecar — FortiClient SSL VPN (openfortivpn)
# Cliente open-source compatível com FortiGate SSL VPN.
# A API compartilha o namespace de rede deste container
# (network_mode: "service:vpn") para alcançar o Oracle.
# ─────────────────────────────────────────────
FROM alpine:3.20

# openfortivpn (cliente FortiGate SSL VPN) + ppp (pppd, interface ppp0)
# busybox do Alpine já fornece ping/ip usados no healthcheck
RUN apk add --no-cache openfortivpn ppp

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
