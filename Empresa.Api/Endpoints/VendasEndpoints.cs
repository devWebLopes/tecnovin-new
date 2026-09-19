using Empresa.Api.Services;
using System.Security.Claims;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do módulo de vendas.
/// Registra geração de relatórios em REGISTRO_RELATORIOS (P3-T08).
/// </summary>
public static class VendasEndpoints
{
    public static void MapVendasEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/v1/vendas")
            .WithTags("Vendas")
            .RequireAuthorization();

        g.MapGet("/analise", GetAnaliseVendasAsync)
            .WithSummary("Análise de vendas");

        g.MapGet("/ranking-clientes", GetRankingClientesAsync)
            .WithSummary("Ranking de clientes");

        g.MapGet("/plano-vendas", GetPlanoVendasResultadoAsync)
            .WithSummary("Plano de vendas vs resultado");

        g.MapGet("/comercial-mi", GetComercialMIAsync)
            .WithSummary("Comercial Mercado Interno");
    }

    private static async Task<IResult> GetAnaliseVendasAsync(
        int empresa, int? estabelecimento,
        IVendasService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetAnaliseVendasAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "VENDAS_ANALISE", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRankingClientesAsync(
        int empresa, int ano,
        IVendasService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetRankingClientesAsync(empresa, ano);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "VENDAS_RANKING_CLIENTES", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPlanoVendasResultadoAsync(
        int empresa, int ano,
        IVendasService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPlanoVendasResultadoAsync(empresa, ano);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "VENDAS_PLANO_RESULTADO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetComercialMIAsync(
        int empresa, int ano,
        IVendasService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetComercialMIAsync(empresa, ano);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "VENDAS_COMERCIAL_MI", GetIp(ctx));
        return Results.Ok(result);
    }

    // ── helpers ───────────────────────────────────────────────────────────
    private static int GetIdUsuario(HttpContext ctx)
        => int.TryParse(ctx.User.FindFirstValue("ID_USUARIO"), out var id) ? id : 0;

    private static string? GetIp(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString();
}