using Empresa.Data;
using Dapper;

namespace Empresa.Worker;

/// <summary>
/// Worker Service — executa tarefas agendadas equivalentes ao Windows Service legado
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Intervalos das tarefas (em minutos)
    private const int IntervaloHealthCheck = 1;
    private const int IntervaloLimpezaSessoes = 60;
    private const int IntervaloAuditoria = 1440;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== Worker Service GestaoNew iniciado ===");

        using var timerHealth = new PeriodicTimer(TimeSpan.FromMinutes(IntervaloHealthCheck));
        using var timerSessoes = new PeriodicTimer(TimeSpan.FromMinutes(IntervaloLimpezaSessoes));
        using var timerAuditoria = new PeriodicTimer(TimeSpan.FromMinutes(IntervaloAuditoria));

        var tasks = new[]
        {
            ExecutarTarefaAsync("HealthCheck", timerHealth, ExecutarHealthCheckAsync, stoppingToken),
            ExecutarTarefaAsync("LimpezaSessoes", timerSessoes, ExecutarLimpezaSessoesAsync, stoppingToken),
            ExecutarTarefaAsync("Auditoria", timerAuditoria, ExecutarAuditoriaAsync, stoppingToken)
        };

        await Task.WhenAll(tasks);
        _logger.LogInformation("=== Worker Service finalizado ===");
    }

    private async Task ExecutarTarefaAsync(
        string nomeTarefa, PeriodicTimer timer,
        Func<CancellationToken, Task> acao, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tarefa {Nome} agendada a cada {Intervalo}min", nomeTarefa, timer.Period.TotalMinutes);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Executando tarefa: {Nome}", nomeTarefa);
                    await acao(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro na tarefa {Nome}", nomeTarefa);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task ExecutarHealthCheckAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbSession>();
        using var conn = db.CreateConnection();
        conn.Open();
        var result = await conn.QueryFirstOrDefaultAsync<string>("SELECT 'WORKER_OK' FROM DUAL");
        _logger.LogInformation("HealthCheck: {Result}", result ?? "FALHA");
    }

    private async Task ExecutarLimpezaSessoesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbSession>();
        using var conn = db.CreateConnection();
        conn.Open();

        var sessoes = await conn.QueryAsync<(int Sid, int Serial)>(@"
            SELECT SID, SERIAL# FROM V$SESSION
            WHERE STATUS='INACTIVE' AND LOGON_TIME<SYSDATE-1
            AND USERNAME IS NOT NULL AND TYPE!='BACKGROUND'");

        int count = 0;
        foreach (var s in sessoes)
        {
            try { await conn.ExecuteAsync($"ALTER SYSTEM KILL SESSION '{s.Sid},{s.Serial}' IMMEDIATE"); count++; }
            catch (Exception ex) { _logger.LogWarning("Erro sessão {Sid},{Serial}: {Msg}", s.Sid, s.Serial, ex.Message); }
        }
        if (count > 0) _logger.LogInformation("{Count} sessões inativas removidas", count);
    }

    private async Task ExecutarAuditoriaAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbSession>();
        using var conn = db.CreateConnection();
        conn.Open();
        await conn.ExecuteAsync("INSERT INTO WORKER_LOG (DATA_EXECUCAO, TAREFA, STATUS) VALUES (SYSDATE, 'AUDITORIA_DIARIA', 'OK')");
    }
}