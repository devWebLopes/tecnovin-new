# 📋 Skill: Serilog Logging — .NET 9

## Sobre
Esta skill define o padrão de logging estruturado para o projeto GestaoNew usando Serilog. Cobre configuração, enriquecimento, níveis de log, sinks e middlewares de request logging.

## Pacotes Necessários

```xml
<!-- Empresa.Api.csproj -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Oracle" Version="1.0.0" /> <!-- Opcional: log direto no Oracle -->
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />
```

## Configuração no Program.cs

```csharp
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

// Configuração Serilog — DEVE ser a PRIMEIRA coisa no Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "GestaoNew")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/gestaonew-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/errors-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: LogEventLevel.Error,
        formatter: new CompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando GestaoNew API");

    var builder = WebApplication.CreateBuilder(args);

    // ⚠️ CRÍTICO: Substituir o logger padrão pelo Serilog
    builder.Host.UseSerilog();

    // ... resto da configuração ...

    var app = builder.Build();

    // ⚠️ Adicionar middleware de request logging ANTES dos endpoints
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} em {Elapsed:0.000} ms";
        options.GetLevel = (ctx, elapsed, ex) =>
        {
            if (ex != null || ctx.Response.StatusCode >= 500)
                return LogEventLevel.Error;
            if (ctx.Response.StatusCode >= 400)
                return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
```

## Configuração via appsettings.json (Alternativa)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System.Net.Http.HttpClient": "Warning"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "GestaoNew"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gestaonew-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

```csharp
// No Program.cs com appsettings
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

## Níveis de Log e Quando Usar

| Nível | Quando Usar | Exemplo |
|-------|-------------|---------|
| `Verbose` | Debug detalhado (dev apenas) | Parâmetros de entrada de query |
| `Debug` | Informação de desenvolvimento | "Mapeando 4 cursores do sp_realizado" |
| `Information` | Operações normais de negócio | "Usuário admin autenticado com sucesso" |
| `Warning` | Situações anômalas mas não críticas | "Tentativa de login com senha incorreta" |
| `Error` | Erros que precisam de atenção | "Falha ao conectar ao Oracle: timeout" |
| `Fatal` | Erros que derrubam a aplicação | "Não foi possível iniciar o Worker Service" |

## Padrões de Log nos Services

```csharp
public class FinanceiroService : IFinanceiroService
{
    private readonly IFinanceiroRepository _repo;
    private readonly ILogger<FinanceiroService> _logger;

    public FinanceiroService(
        IFinanceiroRepository repo,
        ILogger<FinanceiroService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<PosicaoResponse> GetPosicaoFinanceiraAsync(FiltroRequest filtro)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Modulo"] = "Financeiro",
            ["Operacao"] = "GetPosicaoFinanceira",
            ["EmpresaId"] = filtro.IdEmpresa
        });

        _logger.LogInformation(
            "Consultando posição financeira: Ano={Ano}, Empresa={Empresa}, Versao={Versao}",
            filtro.Ano, filtro.IdEmpresa, filtro.Versao);

        try
        {
            var dados = await _repo.GetPosicaoAsync(filtro);

            // ⚠️ NUNCA logar dados sensíveis ou volumes grandes
            _logger.LogInformation(
                "Posição financeira retornou {Registros} registros em {TempoMs}ms",
                dados.Count(), stopwatch.ElapsedMilliseconds);

            return dados;
        }
        catch (OracleException ex) when (ex.Number == 12170) // Timeout
        {
            _logger.LogError(ex,
                "Timeout Oracle ao consultar posição financeira: {ConnectionString}",
                "[REDACTED]");
            throw new ServiceException("Tempo limite excedido. Tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro inesperado ao consultar posição financeira");
            throw;
        }
    }
}
```

## Logging no Middleware de Exceção

```csharp
// Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var requestId = context.TraceIdentifier;
            var userId = context.User.FindFirst("id_usuario")?.Value ?? "anonymous";

            _logger.LogError(ex,
                "🚨 Erro não tratado | RequestId={RequestId} | UserId={UserId} | " +
                "Path={Path} | Method={Method} | StatusCode=500",
                requestId, userId,
                context.Request.Path,
                context.Request.Method);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                mensagem = "Erro interno do servidor",
                requestId = requestId // Expõe ID para rastreamento (nunca stack trace)
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

## Logging no Worker Service

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("🔧 Worker Service iniciado");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Executando tarefas agendadas...");

                // Executa tarefas...

                _logger.LogInformation("Tarefas concluídas. Próxima execução em {Minutos}min", 5);
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Worker Service cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na execução das tarefas do Worker");
                await Task.Delay(TimeSpan.FromMinutes(1), ct); // Backoff
            }
        }
    }
}
```

## Logging Condicional e Performance

```csharp
// ✅ Use LoggerMessage para hot paths (evita boxing e alocação)
public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Usuário {Login} autenticado com sucesso (Perfil={IdPerfil})")]
    public static partial void LogLoginSucesso(
        this ILogger logger, string login, int idPerfil);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Tentativa de login falhou para usuário: {Login}")]
    public static partial void LogLoginFalha(
        this ILogger logger, string login);
}

// Uso: _logger.LogLoginSucesso("admin", 1);
```

## Regras Obrigatórias

1. **SEMPRE** usar structured logging com placeholders `{Propriedade}` — nunca interpolação `$"{valor}"`
2. **SEMPRE** usar `LogContext` com `BeginScope` para adicionar contexto (UserId, RequestId, Módulo)
3. **NUNCA** logar dados sensíveis: senhas, tokens JWT, connection strings completas
4. **SEMPRE** logar `RequestId` em erros para correlação com o cliente
5. **SEMPRE** usar `ILogger<T>` injetado — nunca `Log.Logger` estático
6. **SEMPRE** usar `LoggerMessage` para hot paths (endpoints de alta frequência)
7. **SEMPRE** logar exceção completa (`ex`) no primeiro parâmetro de `LogError`/`LogWarning`
8. **NUNCA** logar em nível `Information` dentro de loops (use `Debug` ou `Verbose`)
9. **SEMPRE** usar `SerilogRequestLogging` middleware como primeiro middleware do pipeline
10. **SEMPRE** configurar rolling file com limite de retenção (evita disco cheio)

## Localização dos Logs

```
Empresa.Api/logs/
├── gestaonew-20260730.log      # Log principal do dia
├── gestaonew-20260729.log      # Dia anterior
├── errors-20260730.json        # Apenas erros, formato JSON
└── errors-20260729.json
```

> ⚠️ Adicionar `logs/` ao `.gitignore` — nunca commitar arquivos de log.

## Logs de Auditoria (Regras de Negócio)

```csharp
public async Task AlterarCfopAsync(int id, CfopRequest request, int idUsuario)
{
    // Log de auditoria: quem alterou o quê e quando
    _logger.LogInformation(
        "🔏 AUDITORIA | CFOP {IdCfop} alterado por Usuario={IdUsuario} | " +
        "Operacao={Operacao} | Timestamp={Timestamp}",
        id, idUsuario, request.Operacao, DateTime.UtcNow);

    // ... executa alteração ...
}