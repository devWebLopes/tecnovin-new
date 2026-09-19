using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public interface IFinanceiroService
{
    Task<IEnumerable<PosicaoFinanceira>> GetPosicaoFinanceiraAsync(int empresa, string versao, DateTime? dataInicial, DateTime? dataFinal);
    Task<IEnumerable<PosicaoFinanceiraResumo>> GetResumoPosicaoAsync(int empresa);
    Task<IEnumerable<PosicaoSemanal>> GetPosicaoSemanalAsync(int empresa);
    Task<IEnumerable<FluxoCaixaMaster>> GetFluxoCaixaAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal);
    Task<IEnumerable<FluxoCaixaDetalhe>> GetFluxoCaixaAnaliticoAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal);
    Task<FluxoCaixaTotais?> GetTotaisFluxoCaixaAsync(int empresa);
    Task<IEnumerable<Dre>> GetDreAsync(int empresa, int ano, string versao);
    Task<IEnumerable<DreDocumento>> GetDreDocumentosAsync(int empresa, string conta, string versao);
    Task<IEnumerable<ProjecaoFinanceira>> GetProjecaoAsync(int empresa, int ano);
    Task<IEnumerable<AjusteFinanceiro>> GetAjustesAsync(int empresa);
    Task<int> CreateAjusteAsync(AjusteFinanceiro ajuste);
    Task<bool> InativarAjusteAsync(int id);
    Task<IEnumerable<Portador>> GetPortadoresAsync();
    Task<bool> SetSaldoInicialAsync(int idPortador, decimal saldo);
}

public class FinanceiroService : IFinanceiroService
{
    private readonly IFinanceiroRepository _repository;
    public FinanceiroService(IFinanceiroRepository repository) => _repository = repository;

    public Task<IEnumerable<PosicaoFinanceira>> GetPosicaoFinanceiraAsync(int empresa, string versao, DateTime? dataInicial, DateTime? dataFinal)
        => _repository.GetPosicaoFinanceiraAsync(empresa, versao, dataInicial, dataFinal);
    public Task<IEnumerable<PosicaoFinanceiraResumo>> GetResumoPosicaoAsync(int empresa) => _repository.GetResumoPosicaoAsync(empresa);
    public Task<IEnumerable<PosicaoSemanal>> GetPosicaoSemanalAsync(int empresa) => _repository.GetPosicaoSemanalAsync(empresa);
    public Task<IEnumerable<FluxoCaixaMaster>> GetFluxoCaixaAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal)
        => _repository.GetFluxoCaixaAsync(empresa, tipo, dataInicial, dataFinal);
    public Task<IEnumerable<FluxoCaixaDetalhe>> GetFluxoCaixaAnaliticoAsync(int empresa, char tipo, DateTime dataInicial, DateTime dataFinal)
        => _repository.GetFluxoCaixaAnaliticoAsync(empresa, tipo, dataInicial, dataFinal);
    public Task<FluxoCaixaTotais?> GetTotaisFluxoCaixaAsync(int empresa) => _repository.GetTotaisFluxoCaixaAsync(empresa);
    public Task<IEnumerable<Dre>> GetDreAsync(int empresa, int ano, string versao) => _repository.GetDreAsync(empresa, ano, versao);
    public Task<IEnumerable<DreDocumento>> GetDreDocumentosAsync(int empresa, string conta, string versao) => _repository.GetDreDocumentosAsync(empresa, conta, versao);
    public Task<IEnumerable<ProjecaoFinanceira>> GetProjecaoAsync(int empresa, int ano) => _repository.GetProjecaoAsync(empresa, ano);
    public Task<IEnumerable<AjusteFinanceiro>> GetAjustesAsync(int empresa) => _repository.GetAjustesAsync(empresa);
    public Task<int> CreateAjusteAsync(AjusteFinanceiro ajuste) => _repository.CreateAjusteAsync(ajuste);
    public Task<bool> InativarAjusteAsync(int id) => _repository.InativarAjusteAsync(id);
    public Task<IEnumerable<Portador>> GetPortadoresAsync() => _repository.GetPortadoresAsync();
    public Task<bool> SetSaldoInicialAsync(int idPortador, decimal saldo) => _repository.SetSaldoInicialAsync(idPortador, saldo);
}