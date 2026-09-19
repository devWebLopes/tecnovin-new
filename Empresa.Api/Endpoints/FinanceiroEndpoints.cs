using Empresa.Data.Models;
using Empresa.Api.Services;
using System.Security.Claims;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do módulo financeiro.
/// Registra geração de relatórios em REGISTRO_RELATORIOS (P3-T08).
/// </summary>
public static class FinanceiroEndpoints
{
    public static void MapFinanceiroEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/v1/financeiro")
            .WithTags("Financeiro")
            .RequireAuthorization();

        // ─── Posição Financeira ───────────────────────
        g.MapGet("/posicao", GetPosicaoAsync)
            .WithSummary("Posição financeira (versões: new, legado, sreal)");

        g.MapGet("/posicao/resumo", GetResumoPosicaoAsync)
            .WithSummary("Resumo da posição financeira (3 cursores)");

        g.MapGet("/posicao/semanal", GetPosicaoSemanalAsync)
            .WithSummary("Posição semanal com remoção de colunas vazias");

        // ─── Fluxo de Caixa ───────────────────────────
        g.MapGet("/fluxo-caixa", GetFluxoCaixaAsync)
            .WithSummary("Fluxo de caixa (master + detalhe)");

        g.MapGet("/fluxo-caixa/analitico", GetFluxoCaixaAnaliticoAsync)
            .WithSummary("Fluxo de caixa analítico");

        g.MapGet("/fluxo-caixa/totais", GetTotaisFluxoCaixaAsync)
            .WithSummary("Totais do fluxo de caixa");

        // ─── DRE ──────────────────────────────────────
        g.MapGet("/dre", GetDreAsync)
            .WithSummary("DRE (versões: padrao, homologado, out)");

        g.MapGet("/dre/documentos", GetDreDocumentosAsync)
            .WithSummary("Documentos do DRE (drill-down conta)");

        // ─── Projeção ─────────────────────────────────
        g.MapGet("/projecao", GetProjecaoAsync)
            .WithSummary("Projeção financeira anual");

        // ─── Ajustes ──────────────────────────────────
        g.MapGet("/ajustes", GetAjustesAsync)
            .WithSummary("Lista ajustes financeiros ativos");

        g.MapPost("/ajustes", CreateAjusteAsync)
            .WithSummary("Cria um ajuste financeiro");

        g.MapDelete("/ajustes/{id:int}", InativarAjusteAsync)
            .WithSummary("Inativa um ajuste financeiro");

        // ─── Portador ─────────────────────────────────
        g.MapGet("/portadores", GetPortadoresAsync)
            .WithSummary("Lista portadores para saldo");

        g.MapPost("/portadores/saldo-inicial", SetSaldoInicialAsync)
            .WithSummary("Define saldo inicial de portador");
    }

    private static async Task<IResult> GetPosicaoAsync(
        int empresa, string? versao, DateTime? dataInicial, DateTime? dataFinal,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPosicaoFinanceiraAsync(empresa, versao ?? "new", dataInicial, dataFinal);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), $"FINANCEIRO_POSICAO_{(versao ?? "new").ToUpper()}", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetResumoPosicaoAsync(
        int empresa,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetResumoPosicaoAsync(empresa);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_RESUMO_POSICAO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPosicaoSemanalAsync(
        int empresa,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPosicaoSemanalAsync(empresa);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_POSICAO_SEMANAL", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetFluxoCaixaAsync(
        int empresa, char tipo, DateTime dataInicial, DateTime dataFinal,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetFluxoCaixaAsync(empresa, tipo, dataInicial, dataFinal);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_FLUXO_CAIXA", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetFluxoCaixaAnaliticoAsync(
        int empresa, char tipo, DateTime dataInicial, DateTime dataFinal,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetFluxoCaixaAnaliticoAsync(empresa, tipo, dataInicial, dataFinal);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_FLUXO_CAIXA_ANALITICO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTotaisFluxoCaixaAsync(
        int empresa,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetTotaisFluxoCaixaAsync(empresa);
        if (result is not null)
            await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_FLUXO_CAIXA_TOTAIS", GetIp(ctx));
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetDreAsync(
        int empresa, int ano, string? versao,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetDreAsync(empresa, ano, versao ?? "padrao");
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), $"FINANCEIRO_DRE_{(versao ?? "padrao").ToUpper()}", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetDreDocumentosAsync(
        int empresa, string conta, string? versao,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetDreDocumentosAsync(empresa, conta, versao ?? "padrao");
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_DRE_DRILL_DOWN", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProjecaoAsync(
        int empresa, int ano,
        IFinanceiroService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetProjecaoAsync(empresa, ano);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "FINANCEIRO_PROJECAO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAjustesAsync(int empresa, IFinanceiroService s)
        => Results.Ok(await s.GetAjustesAsync(empresa));

    private static async Task<IResult> CreateAjusteAsync(AjusteFinanceiro ajuste, IFinanceiroService s)
    {
        var id = await s.CreateAjusteAsync(ajuste);
        return Results.Created($"/api/v1/financeiro/ajustes/{id}", new { id });
    }

    private static async Task<IResult> InativarAjusteAsync(int id, IFinanceiroService s)
        => await s.InativarAjusteAsync(id) ? Results.NoContent() : Results.NotFound();

    private static async Task<IResult> GetPortadoresAsync(IFinanceiroService s)
        => Results.Ok(await s.GetPortadoresAsync());

    private static async Task<IResult> SetSaldoInicialAsync(int idPortador, decimal saldo, IFinanceiroService s)
        => await s.SetSaldoInicialAsync(idPortador, saldo) ? Results.Ok() : Results.Problem("Erro ao definir saldo");

    // ── helpers ───────────────────────────────────────────────────────────
    private static int GetIdUsuario(HttpContext ctx)
        => int.TryParse(ctx.User.FindFirstValue("ID_USUARIO"), out var id) ? id : 0;

    private static string? GetIp(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString();
}