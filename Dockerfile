# ─────────────────────────────────────────────
# Dockerfile — Empresa.Api (.NET 9)
# Multi-stage build: restore → build → publish → runtime
# ─────────────────────────────────────────────

# Stage 1: Restore
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY Empresa.sln ./
COPY Empresa.Api/Empresa.Api.csproj Empresa.Api/
COPY Empresa.Data/Empresa.Data.csproj Empresa.Data/
COPY Empresa.Util/Empresa.Util.csproj Empresa.Util/
COPY Empresa.Worker/Empresa.Worker.csproj Empresa.Worker/
RUN dotnet restore

# Stage 2: Build
FROM restore AS build
COPY . .
WORKDIR /src/Empresa.Api
RUN dotnet build -c Release --no-restore

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-build

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Empresa.Api.dll"]