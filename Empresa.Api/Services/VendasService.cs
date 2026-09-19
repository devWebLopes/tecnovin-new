using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public interface IVendasService
{
    Task<IEnumerable<AnaliseVendas>> GetAnaliseVendasAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<RankingCliente>> GetRankingClientesAsync(int empresa, int ano);
    Task<IEnumerable<PlanoVendasResultado>> GetPlanoVendasResultadoAsync(int empresa, int ano);
    Task<IEnumerable<ComercialMI>> GetComercialMIAsync(int empresa, int ano);
}

public class VendasService : IVendasService
{
    private readonly IVendasRepository _repository;
    public VendasService(IVendasRepository repository) => _repository = repository;

    public Task<IEnumerable<AnaliseVendas>> GetAnaliseVendasAsync(int empresa, int? estabelecimento)
        => _repository.GetAnaliseVendasAsync(empresa, estabelecimento);
    public Task<IEnumerable<RankingCliente>> GetRankingClientesAsync(int empresa, int ano)
        => _repository.GetRankingClientesAsync(empresa, ano);
    public Task<IEnumerable<PlanoVendasResultado>> GetPlanoVendasResultadoAsync(int empresa, int ano)
        => _repository.GetPlanoVendasResultadoAsync(empresa, ano);
    public Task<IEnumerable<ComercialMI>> GetComercialMIAsync(int empresa, int ano)
        => _repository.GetComercialMIAsync(empresa, ano);
}