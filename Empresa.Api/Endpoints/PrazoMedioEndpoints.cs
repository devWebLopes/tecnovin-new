using Empresa.Api.Services;
using System.Security.Claims;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do módulo de prazo médio — 3 níveis de drill-down recebimento e pagamento.
/// Registra geração de relatórios em REGISTRO_RELATORIOS (P3-T08).
/// </summary>
public static class PrazoMedioEndpoints
{
    public static void MapPrazoMedioEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/v1/prazo-medio")
            .WithTags("Prazo Médio")
            .RequireAuthorization();

        // ─── RECEBIMENTO ─────────────────────────────────
        g.MapGet("/recebimento/mensal", GetRecebimentoMensalAsync)
            .WithSummary("Prazo médio de recebimento mensal (Nível 1)");

        g.MapGet("/recebimento/pessoa", GetRecebimentoPessoaAsync)
            .WithSummary("Prazo médio de recebimento por pessoa (Nível 2)");

        g.MapGet("/recebimento/documentos", GetRecebimentoDocumentosAsync)
            .WithSummary("Documentos de recebimento (Nível 3 - drill-down)");

        // ─── PAGAMENTO ───────────────────────────────────
        g.MapGet("/pagamento/mensal", GetPagamentoMensalAsync)
            .WithSummary("Prazo médio de pagamento mensal (Nível 1)");

        g.MapGet("/pagamento/pessoa", GetPagamentoPessoaAsync)
            .WithSummary("Prazo médio de pagamento por pessoa (Nível 2)");

        g.MapGet("/pagamento/documentos", GetPagamentoDocumentosAsync)
            .WithSummary("Documentos de pagamento (Nível 3 - drill-down)");
    }

    private static async Task<IResult> GetRecebimentoMensalAsync(
        int empresa, int ano,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetRecebimentoMensalAsync(empresa, ano);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_RECEBIMENTO_MENSAL", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRecebimentoPessoaAsync(
        int empresa, int ano, string mes,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetRecebimentoPessoaAsync(empresa, ano, mes);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_RECEBIMENTO_PESSOA", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRecebimentoDocumentosAsync(
        int empresa, string pessoa, int ano, string mes,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetRecebimentoDocumentosAsync(empresa, pessoa, ano, mes);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_RECEBIMENTO_DOCS", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPagamentoMensalAsync(
        int empresa, int ano, string? tipoOperacao,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPagamentoMensalAsync(empresa, ano, tipoOperacao);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_PAGAMENTO_MENSAL", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPagamentoPessoaAsync(
        int empresa, int ano, string mes,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPagamentoPessoaAsync(empresa, ano, mes);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_PAGAMENTO_PESSOA", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPagamentoDocumentosAsync(
        int empresa, string pessoa, int ano, string mes,
        IPrazoMedioService s, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await s.GetPagamentoDocumentosAsync(empresa, pessoa, ano, mes);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "PRAZO_MEDIO_PAGAMENTO_DOCS", GetIp(ctx));
        return Results.Ok(result);
    }

    // ── helpers ───────────────────────────────────────────────────────────
    private static int GetIdUsuario(HttpContext ctx)
        => int.TryParse(ctx.User.FindFirstValue("ID_USUARIO"), out var id) ? id : 0;

    private static string? GetIp(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString();
}