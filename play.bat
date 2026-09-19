@echo off
echo Iniciando os servicos...

:: Abre o primeiro terminal, vai para a pasta da API e roda o dotnet run
start "Empresa.Api - Dotnet" /D "D:\clientes\go19\tecnovin\teste\GestaoNew\Empresa.Api" cmd /k "dotnet run"

:: Abre o segundo terminal, vai para a pasta do Frontend e roda o npm run dev
start "GestaoNew - Frontend" /D "D:\clientes\go19\tecnovin\teste\GestaoNew\Empresa.Web" cmd /k "npm run dev"

echo Terminais abertos com sucesso!