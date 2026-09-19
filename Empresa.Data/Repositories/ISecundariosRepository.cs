using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

public interface IAgricolaRepository
{
    Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasAsync(DateTime data);
    Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasDetAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada);
    Task<(IReadOnlyList<string> Colunas, IEnumerable<IDictionary<string, object>> Linhas)> GetRecebimentoFrutasDetNfAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, string? colunaGrauClicada, string? cdVariedade);
}

public interface ICalendarioRepository
{
    Task<IEnumerable<CalendarioFinanceiro>> GetDatasAsync(int empresa, int ano, int mes);
    Task<bool> UpdateDiaUtilAsync(DateTime data, bool util);
}

public interface IAgrupamentoRepository
{
    Task<IEnumerable<AgrupamentoDre>> GetAllAsync();
    Task<int> CreateAsync(AgrupamentoDre item);
    Task<bool> DeleteAsync(int id);
}

public interface IDbaRepository
{
    Task<IEnumerable<SessaoOracle>> GetSessoesAsync();
    Task<bool> MatarSessaoAsync(int sid, int serial);
    Task<IEnumerable<TabelaLock>> GetLocksAsync();
}