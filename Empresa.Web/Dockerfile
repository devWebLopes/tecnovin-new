# ─────────────────────────────────────────────
# Dockerfile — Empresa.Web (React 18 + Vite + Nginx)
# Multi-stage build: node build → nginx runtime
# ─────────────────────────────────────────────

# Stage 1: Build da SPA
FROM node:20-alpine AS build
WORKDIR /app

# Instalação de dependências
COPY package*.json ./
RUN npm ci

# Copia código-fonte e compilação
COPY . .
RUN npm run build

# Stage 2: Runtime Nginx
FROM nginx:1.27-alpine AS runtime

# Remove configuração padrão
RUN rm -rf /etc/nginx/conf.d/default.conf

# Copia a configuração SPA e os arquivos compilados
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget -qO- http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
