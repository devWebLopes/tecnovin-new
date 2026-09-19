using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public interface IComprasService
{
    Task<IEnumerable<ResumoAnualCompras>> GetResumoAnualAsync(int empresa, int? estabelecimento);
    Task<ResumoAnualComprasTotais?> GetResumoAnualTotaisAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<ResumoAnualComprasGrafico>> GetResumoAnualGraficoAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<ComiteComprasNF>> GetComiteComprasNFAsync(int empresa, DateTime dataInicial, DateTime dataFinal);
    Task<IEnumerable<PrevisaoCompra>> GetPrevistoRealizadoAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<ProgressaoPrecoNF>> GetProgressaoPrecoAsync(int empresa, string? produto);
    Task<IEnumerable<CfopTransferencia>> GetCfopTransferenciasAsync();
    Task<int> CreateCfopTransferenciaAsync(CfopTransferencia cfop);
    Task<bool> DeleteCfopTransferenciaAsync(int id);
    Task<IEnumerable<CentroCusto>> GetCentroCustoAsync(int empresa, int? estabelecimento);
    Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheAsync(string conta, int? estabelecimento);
    Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheProdutoAsync(string conta, int? estabelecimento);
}

public class ComprasService : IComprasService
{
    private readonly IComprasRepository _repository;
    private readonly ILogger<ComprasService> _logger;

    public ComprasService(IComprasRepository repository, ILogger<ComprasService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IEnumerable<ResumoAnualCompras>> GetResumoAnualAsync(int empresa, int? estabelecimento)
        => _repository.GetResumoAnualAsync(empresa, estabelecimento);

    public Task<ResumoAnualComprasTotais?> GetResumoAnualTotaisAsync(int empresa, int? estabelecimento)
        => _repository.GetResumoAnualTotaisAsync(empresa, estabelecimento);

    public Task<IEnumerable<ResumoAnualComprasGrafico>> GetResumoAnualGraficoAsync(int empresa, int? estabelecimento)
        => _repository.GetResumoAnualGraficoAsync(empresa, estabelecimento);

    public Task<IEnumerable<ComiteComprasNF>> GetComiteComprasNFAsync(int empresa, DateTime dataInicial, DateTime dataFinal)
        => _repository.GetComiteComprasNFAsync(empresa, dataInicial, dataFinal);

    public Task<IEnumerable<PrevisaoCompra>> GetPrevistoRealizadoAsync(int empresa, int? estabelecimento)
        => _repository.GetPrevistoRealizadoAsync(empresa, estabelecimento);

    public Task<IEnumerable<ProgressaoPrecoNF>> GetProgressaoPrecoAsync(int empresa, string? produto)
        => _repository.GetProgressaoPrecoAsync(empresa, produto);

    public Task<IEnumerable<CfopTransferencia>> GetCfopTransferenciasAsync()
        => _repository.GetCfopTransferenciasAsync();

    public Task<int> CreateCfopTransferenciaAsync(CfopTransferencia cfop)
        => _repository.CreateCfopTransferenciaAsync(cfop);

    public Task<bool> DeleteCfopTransferenciaAsync(int id)
        => _repository.DeleteCfopTransferenciaAsync(id);

    public Task<IEnumerable<CentroCusto>> GetCentroCustoAsync(int empresa, int? estabelecimento)
        => _repository.GetCentroCustoAsync(empresa, estabelecimento);

    public Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheAsync(string conta, int? estabelecimento)
        => _repository.GetCentroCustoDetalheAsync(conta, estabelecimento);

    public Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheProdutoAsync(string conta, int? estabelecimento)
        => _repository.GetCentroCustoDetalheProdutoAsync(conta, estabelecimento);
}