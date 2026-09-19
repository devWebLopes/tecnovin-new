using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

public interface IVendasRepository
{
    Task<IEnumerable<AnaliseVendas>> GetAnaliseVendasAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<RankingCliente>> GetRankingClientesAsync(int empresa, int ano);
    Task<IEnumerable<PlanoVendasResultado>> GetPlanoVendasResultadoAsync(int empresa, int ano);
    Task<IEnumerable<ComercialMI>> GetComercialMIAsync(int empresa, int ano);
}