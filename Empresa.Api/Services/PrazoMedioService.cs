using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

public interface IPrazoMedioService
{
    Task<IEnumerable<PrazoMedioMensal>> GetRecebimentoMensalAsync(int empresa, int ano);
    Task<IEnumerable<PrazoMedioPessoa>> GetRecebimentoPessoaAsync(int empresa, int ano, string mes);
    Task<IEnumerable<PrazoMedioDocumento>> GetRecebimentoDocumentosAsync(int empresa, string pessoa, int ano, string mes);
    Task<IEnumerable<PrazoMedioMensal>> GetPagamentoMensalAsync(int empresa, int ano, string? tipoOperacao);
    Task<IEnumerable<PrazoMedioPessoa>> GetPagamentoPessoaAsync(int empresa, int ano, string mes);
    Task<IEnumerable<PrazoMedioDocumento>> GetPagamentoDocumentosAsync(int empresa, string pessoa, int ano, string mes);
}

public class PrazoMedioService : IPrazoMedioService
{
    private readonly IPrazoMedioRepository _repository;
    public PrazoMedioService(IPrazoMedioRepository repository) => _repository = repository;

    public Task<IEnumerable<PrazoMedioMensal>> GetRecebimentoMensalAsync(int empresa, int ano)
        => _repository.GetRecebimentoMensalAsync(empresa, ano);
    public Task<IEnumerable<PrazoMedioPessoa>> GetRecebimentoPessoaAsync(int empresa, int ano, string mes)
        => _repository.GetRecebimentoPessoaAsync(empresa, ano, mes);
    public Task<IEnumerable<PrazoMedioDocumento>> GetRecebimentoDocumentosAsync(int empresa, string pessoa, int ano, string mes)
        => _repository.GetRecebimentoDocumentosAsync(empresa, pessoa, ano, mes);
    public Task<IEnumerable<PrazoMedioMensal>> GetPagamentoMensalAsync(int empresa, int ano, string? tipoOperacao)
        => _repository.GetPagamentoMensalAsync(empresa, ano, tipoOperacao);
    public Task<IEnumerable<PrazoMedioPessoa>> GetPagamentoPessoaAsync(int empresa, int ano, string mes)
        => _repository.GetPagamentoPessoaAsync(empresa, ano, mes);
    public Task<IEnumerable<PrazoMedioDocumento>> GetPagamentoDocumentosAsync(int empresa, string pessoa, int ano, string mes)
        => _repository.GetPagamentoDocumentosAsync(empresa, pessoa, ano, mes);
}