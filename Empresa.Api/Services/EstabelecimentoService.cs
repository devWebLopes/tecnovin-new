using Empresa.Api.DTOs;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public interface IEstabelecimentoService
{
    Task<IEnumerable<EstabelecimentoResponse>> GetAllAsync();
    Task<EstabelecimentoResponse?> GetByIdAsync(int id);
    Task<EstabelecimentoResponse> CreateAsync(EstabelecimentoRequest request);
    Task<bool> UpdateAsync(int id, EstabelecimentoRequest request);
    Task<bool> DeleteAsync(int id);
}

public class EstabelecimentoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public bool Ativo { get; set; } = true;
}

public class EstabelecimentoService : IEstabelecimentoService
{
    private readonly IEstabelecimentoRepository _repository;
    private readonly ILogger<EstabelecimentoService> _logger;

    public EstabelecimentoService(IEstabelecimentoRepository repository, ILogger<EstabelecimentoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<EstabelecimentoResponse>> GetAllAsync()
    {
        var estabelecimentos = await _repository.GetAllAsync();
        return estabelecimentos.Select(MapToResponse);
    }

    public async Task<EstabelecimentoResponse?> GetByIdAsync(int id)
    {
        var estabelecimento = await _repository.GetByIdAsync(id);
        return estabelecimento is null ? null : MapToResponse(estabelecimento);
    }

    public async Task<EstabelecimentoResponse> CreateAsync(EstabelecimentoRequest request)
    {
        var estabelecimento = new Estabelecimento
        {
            Nome = request.Nome,
            Cnpj = request.Cnpj,
            Ativo = request.Ativo ? "S" : "N"
        };

        var id = await _repository.CreateAsync(estabelecimento);
        estabelecimento.IDEstabelecimento = id;

        _logger.LogInformation("Estabelecimento {Nome} (ID: {Id}) criado com sucesso", estabelecimento.Nome, id);
        return MapToResponse(estabelecimento);
    }

    public async Task<bool> UpdateAsync(int id, EstabelecimentoRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException("Estabelecimento", id);

        existing.Nome = request.Nome;
        existing.Cnpj = request.Cnpj;
        existing.Ativo = request.Ativo ? "S" : "N";

        return await _repository.UpdateAsync(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static EstabelecimentoResponse MapToResponse(Estabelecimento est)
    {
        return new EstabelecimentoResponse
        {
            Id = est.IDEstabelecimento,
            Nome = est.Nome,
            Cnpj = est.Cnpj,
            Ativo = est.Ativo == "S"
        };
    }
}