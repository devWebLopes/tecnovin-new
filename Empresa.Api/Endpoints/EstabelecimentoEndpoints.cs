using Empresa.Api.DTOs;
using Empresa.Api.Services;

namespace Empresa.Api.Endpoints;

/// <summary>
/// Endpoints de CRUD para estabelecimentos
/// </summary>
public static class EstabelecimentoEndpoints
{
    public static void MapEstabelecimentoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/estabelecimentos")
            .WithTags("Estabelecimentos");

        grupo.MapGet("/", GetAllAsync)
            .WithSummary("Lista todos os estabelecimentos")
            .Produces<IEnumerable<EstabelecimentoResponse>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:int}", GetByIdAsync)
            .WithSummary("Busca estabelecimento por ID")
            .Produces<EstabelecimentoResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPost("/", CreateAsync)
            .WithSummary("Cria um novo estabelecimento")
            .Produces<EstabelecimentoResponse>(StatusCodes.Status201Created);

        grupo.MapPut("/{id:int}", UpdateAsync)
            .WithSummary("Atualiza um estabelecimento existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapDelete("/{id:int}", DeleteAsync)
            .WithSummary("Exclui logicamente um estabelecimento")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllAsync(IEstabelecimentoService service)
    {
        var estabelecimentos = await service.GetAllAsync();
        return Results.Ok(estabelecimentos);
    }

    private static async Task<IResult> GetByIdAsync(int id, IEstabelecimentoService service)
    {
        var estabelecimento = await service.GetByIdAsync(id);
        return estabelecimento is null ? Results.NotFound() : Results.Ok(estabelecimento);
    }

    private static async Task<IResult> CreateAsync(EstabelecimentoRequest request, IEstabelecimentoService service)
    {
        var estabelecimento = await service.CreateAsync(request);
        return Results.Created($"/api/v1/estabelecimentos/{estabelecimento.Id}", estabelecimento);
    }

    private static async Task<IResult> UpdateAsync(int id, EstabelecimentoRequest request, IEstabelecimentoService service)
    {
        var result = await service.UpdateAsync(id, request);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeleteAsync(int id, IEstabelecimentoService service)
    {
        var result = await service.DeleteAsync(id);
        return result ? Results.NoContent() : Results.NotFound();
    }
}