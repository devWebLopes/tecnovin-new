using Empresa.Data.Models;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

public static class SecundariosEndpoints
{
    public static void MapSecundariosEndpoints(this WebApplication app)
    {
        // ⚠️ Grupo "Agrícola" removido — substituído por AgricolaEndpoints.cs (Fase 19)
        // O stub antigo usava assinatura incorreta, string mágica de package e model incompatível.
        // Ver PRD_AGRICOLA.md §5.3 para detalhes da quebra de contrato.

        // Calendario
        var cal = app.MapGroup("/api/v1/calendario").WithTags("Calendario Financeiro");
        cal.MapGet("/", async (int empresa, int ano, int mes, ICalendarioService s) =>
            Results.Ok(await s.GetDatasAsync(empresa, ano, mes)))
            .WithSummary("Datas do calendario financeiro");
        cal.MapPut("/", async (DateTime data, bool util, ICalendarioService s) =>
            await s.UpdateDiaUtilAsync(data, util) ? Results.NoContent() : Results.NotFound())
            .WithSummary("Atualiza dia util");

        // Agrupamentos DRE
        var agp = app.MapGroup("/api/v1/agrupamentos").WithTags("Agrupamentos DRE");
        agp.MapGet("/", async (IAgrupamentoService s) => Results.Ok(await s.GetAllAsync()))
            .WithSummary("Lista agrupamentos DRE");
        agp.MapPost("/", async (AgrupamentoDre item, IAgrupamentoService s) =>
        {
            var id = await s.CreateAsync(item);
            return Results.Created($"/api/v1/agrupamentos/{id}", new { id });
        }).WithSummary("Cria agrupamento DRE");
        agp.MapDelete("/{id:int}", async (int id, IAgrupamentoService s) =>
            await s.DeleteAsync(id) ? Results.NoContent() : Results.NotFound())
            .WithSummary("Remove agrupamento DRE");

        // DBA
        var dba = app.MapGroup("/api/v1/dba").WithTags("DBA").RequireAuthorization();
        dba.MapGet("/sessoes", async (IDbaService s) => Results.Ok(await s.GetSessoesAsync()))
            .WithSummary("Sessoes Oracle ativas");
        dba.MapDelete("/sessoes/{sid:int}/{serial:int}", async (int sid, int serial, IDbaService s) =>
            await s.MatarSessaoAsync(sid, serial) ? Results.NoContent() : Results.Problem("Falha ao matar sessao"))
            .WithSummary("Mata sessao Oracle");
        dba.MapGet("/locks", async (IDbaService s) => Results.Ok(await s.GetLocksAsync()))
            .WithSummary("Locks ativos no banco");
    }
}
