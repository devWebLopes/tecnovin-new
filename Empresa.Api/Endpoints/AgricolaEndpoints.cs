using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Empresa.Api.Endpoints;

public static class AgricolaEndpoints
{
    public static void MapAgricolaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/agricola")
            .RequireAuthorization()
            .WithTags("Agrícola")
            .WithSummary("Módulo Agrícola — Cad. Safra/Meta e Compras Frutas")
            .WithDescription("CRUD de metas de compra de frutas por safra e painel analítico de recebimento de frutas via package Oracle pkg_bi_compras.");

        // --- Painel 1: Cad. Safra/Meta ---

        group.MapGet("/metas", async (int idPerfil, IMetaCompraService service) =>
        {
            try
            {
                var metas = await service.GetAllAsync(idPerfil);
                return Results.Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithSummary("Lista todas as metas de compra")
        .WithDescription("Retorna as metas da tabela META_COMPRAS ordenadas por SAFRA DESC.")
        .Produces<List<MetaCompraResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/metas/empresas", async (int idPerfil, IMetaCompraService service) =>
        {
            try
            {
                var empresas = await service.GetEmpresasAsync(idPerfil);
                return Results.Ok(empresas);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithSummary("Domínio fixo de empresas")
        .WithDescription("Retorna as 4 empresas fixas do legado: TECNOVIN, SUVALAN, SUMABRAS, MAISONFORESTIER.")
        .Produces<List<EmpresaMetaResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/metas", async (MetaCompraRequest request, int idPerfil, IMetaCompraService service) =>
        {
            try
            {
                var result = await service.CreateAsync(request, idPerfil);
                return Results.Created($"/api/v1/agricola/metas/{result.IdMetaCompra}", result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Results.UnprocessableEntity(new { error = ex.Message });
            }
        })
        .WithSummary("Cria uma nova meta")
        .WithDescription("Cria uma meta de compra de frutas com validações server-side.")
        .Produces<MetaCompraResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/metas/{id:long}", async (long id, MetaCompraRequest request, int idPerfil, IMetaCompraService service) =>
        {
            try
            {
                var result = await service.UpdateAsync(id, request, idPerfil);
                return result ? Results.NoContent() : Results.NotFound();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Results.UnprocessableEntity(new { error = ex.Message });
            }
        })
        .WithSummary("Atualiza uma meta existente")
        .WithDescription("Atualiza todos os campos editáveis de uma meta. ID_META_COMPRA nunca é editável.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/metas/{id:long}", async (long id, int idPerfil, IMetaCompraService service) =>
        {
            try
            {
                var result = await service.DeleteAsync(id, idPerfil);
                return result ? Results.NoContent() : Results.NotFound();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithSummary("Exclui uma meta")
        .WithDescription("Exclusão física com log Serilog de auditoria.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        // --- Painel 2: Compras Frutas ---

        group.MapGet("/compras-frutas", async (DateTime? data, int idPerfil, IAgricolaService service) =>
        {
            try
            {
                var result = await service.GetRecebimentoFrutasAsync(data, idPerfil);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithSummary("Painel principal de Compras Frutas")
        .WithDescription("Grid dinâmica alimentada pela procedure pkg_bi_compras.sp_recebimento_frutas.")
        .Produces<GridDinamicaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/compras-frutas/detalhamento", async (
            DateTime data, string empresa, string linha, string uf, string colunaClicada, int idPerfil, IAgricolaService service) =>
        {
            try
            {
                var result = await service.GetDetalhamentoAsync(data, empresa, linha, uf, colunaClicada, idPerfil);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithSummary("Drill-down nível 1 — Detalhamento por variedade/grau")
        .WithDescription("Procedure pkg_bi_compras.sp_recebimento_frutas_det.")
        .Produces<GridDinamicaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/compras-frutas/notas-fiscais", async (
            DateTime data, string empresa, string linha, string uf, string colunaClicada,
            string? colunaGrauClicada, string? variedade, int idPerfil, IAgricolaService service) =>
        {
            try
            {
                var result = await service.GetNotasFiscaisAsync(data, empresa, linha, uf, colunaClicada, colunaGrauClicada, variedade, idPerfil);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithSummary("Drill-down nível 2 — Notas fiscais")
        .WithDescription("Procedure pkg_bi_compras.sp_recebimento_frutas_det_nf.")
        .Produces<GridDinamicaResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
