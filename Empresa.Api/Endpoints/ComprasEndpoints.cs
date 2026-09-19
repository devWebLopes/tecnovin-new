using Empresa.Data.Models;
using Empresa.Api.Services;
using System.Security.Claims;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints do módulo de compras — 12 endpoints conforme doc_legado.
/// Registra geração de relatórios em REGISTRO_RELATORIOS (P3-T08).
/// </summary>
public static class ComprasEndpoints
{
    public static void MapComprasEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/compras")
            .WithTags("Compras")
            .RequireAuthorization();

        // GET /api/v1/compras/resumo-anual
        grupo.MapGet("/resumo-anual", GetResumoAnualAsync)
            .WithSummary("Resumo anual de compras com 4 cursores")
            .Produces(StatusCodes.Status200OK);

        // GET /api/v1/compras/resumo-anual/totais
        grupo.MapGet("/resumo-anual/totais", GetResumoAnualTotaisAsync)
            .WithSummary("Totais do resumo anual de compras");

        // GET /api/v1/compras/resumo-anual/grafico
        grupo.MapGet("/resumo-anual/grafico", GetResumoAnualGraficoAsync)
            .WithSummary("Dados gráficos do resumo anual de compras");

        // GET /api/v1/compras/comite
        grupo.MapGet("/comite", GetComiteComprasNFAsync)
            .WithSummary("Comitê de compras por NF");

        // GET /api/v1/compras/previsto-realizado
        grupo.MapGet("/previsto-realizado", GetPrevistoRealizadoAsync)
            .WithSummary("Previsto vs Realizado de compras");

        // GET /api/v1/compras/progressao-preco
        grupo.MapGet("/progressao-preco", GetProgressaoPrecoAsync)
            .WithSummary("Progressão de preços por produto");

        // GET /api/v1/compras/cfop
        grupo.MapGet("/cfop", GetCfopTransferenciasAsync)
            .WithSummary("Lista CFOPs de transferência");

        // POST /api/v1/compras/cfop
        grupo.MapPost("/cfop", CreateCfopTransferenciaAsync)
            .WithSummary("Adiciona CFOP de transferência");

        // DELETE /api/v1/compras/cfop/{id}
        grupo.MapDelete("/cfop/{id:int}", DeleteCfopTransferenciaAsync)
            .WithSummary("Remove CFOP de transferência");

        // GET /api/v1/compras/centro-custo
        grupo.MapGet("/centro-custo", GetCentroCustoAsync)
            .WithSummary("Agrupamento por centro de custo");

        // GET /api/v1/compras/centro-custo/detalhe
        grupo.MapGet("/centro-custo/detalhe", GetCentroCustoDetalheAsync)
            .WithSummary("Detalhe do centro de custo por conta");

        // GET /api/v1/compras/centro-custo/detalhe-produto
        grupo.MapGet("/centro-custo/detalhe-produto", GetCentroCustoDetalheProdutoAsync)
            .WithSummary("Detalhe por produto do centro de custo");
    }

    private static async Task<IResult> GetResumoAnualAsync(
        int empresa, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetResumoAnualAsync(empresa, estabelecimento);
        var totais = await service.GetResumoAnualTotaisAsync(empresa, estabelecimento);
        var grafico = await service.GetResumoAnualGraficoAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_RESUMO_ANUAL", GetIp(ctx));
        return Results.Ok(new { dados = result, totais, grafico });
    }

    private static async Task<IResult> GetResumoAnualTotaisAsync(
        int empresa, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetResumoAnualTotaisAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_RESUMO_ANUAL_TOTAIS", GetIp(ctx));
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetResumoAnualGraficoAsync(
        int empresa, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetResumoAnualGraficoAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_RESUMO_ANUAL_GRAFICO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetComiteComprasNFAsync(
        int empresa, DateTime dataInicial, DateTime dataFinal,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetComiteComprasNFAsync(empresa, dataInicial, dataFinal);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_COMITE_NF", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPrevistoRealizadoAsync(
        int empresa, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetPrevistoRealizadoAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_PREVISTO_REALIZADO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProgressaoPrecoAsync(
        int empresa, string? produto,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetProgressaoPrecoAsync(empresa, produto);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_PROGRESSAO_PRECO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCfopTransferenciasAsync(IComprasService service)
    {
        var result = await service.GetCfopTransferenciasAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateCfopTransferenciaAsync(CfopTransferencia cfop, IComprasService service)
    {
        var id = await service.CreateCfopTransferenciaAsync(cfop);
        return Results.Created($"/api/v1/compras/cfop/{id}", new { id });
    }

    private static async Task<IResult> DeleteCfopTransferenciaAsync(int id, IComprasService service)
    {
        var result = await service.DeleteCfopTransferenciaAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetCentroCustoAsync(
        int empresa, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetCentroCustoAsync(empresa, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_CENTRO_CUSTO", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCentroCustoDetalheAsync(
        string conta, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetCentroCustoDetalheAsync(conta, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_CENTRO_CUSTO_DETALHE", GetIp(ctx));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCentroCustoDetalheProdutoAsync(
        string conta, int? estabelecimento,
        IComprasService service, IRelatorioAuditoriaService auditoria, HttpContext ctx)
    {
        var result = await service.GetCentroCustoDetalheProdutoAsync(conta, estabelecimento);
        await auditoria.RegistrarAsync(GetIdUsuario(ctx), "COMPRAS_CENTRO_CUSTO_PRODUTO", GetIp(ctx));
        return Results.Ok(result);
    }

    // ── helpers ───────────────────────────────────────────────────────────
    private static int GetIdUsuario(HttpContext ctx)
        => int.TryParse(ctx.User.FindFirstValue("ID_USUARIO"), out var id) ? id : 0;

    private static string? GetIp(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString();
}