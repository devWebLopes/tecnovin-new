using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Data.Models;
using Empresa.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Empresa.Api.Services;

// --- MetaCompra ---
public interface IMetaCompraService
{
    Task<List<MetaCompraResponse>> GetAllAsync(int idPerfil);
    Task<List<EmpresaMetaResponse>> GetEmpresasAsync(int idPerfil);
    Task<MetaCompraResponse> CreateAsync(MetaCompraRequest request, int idPerfil);
    Task<bool> UpdateAsync(long id, MetaCompraRequest request, int idPerfil);
    Task<bool> DeleteAsync(long id, int idPerfil);
}

public class MetaCompraService : IMetaCompraService
{
    private readonly IMetaCompraRepository _repository;
    private readonly IAcessoRepository _acessoRepository;

    private static readonly List<EmpresaMetaResponse> EmpresasFixas = new()
    {
        new() { CdEmpresa = 2, Nome = "TECNOVIN DO BRASIL LTDA" },
        new() { CdEmpresa = 200, Nome = "SUVALAN SUCOS DE FRUTAS, INDUSTRIA E COM" },
        new() { CdEmpresa = 300, Nome = "SUMABRAS DO BRASIL LTDA" },
        new() { CdEmpresa = 700, Nome = "MAISONFORESTIER" }
    };

    public MetaCompraService(IMetaCompraRepository repository, IAcessoRepository acessoRepository)
    {
        _repository = repository;
        _acessoRepository = acessoRepository;
    }

    public async Task<List<MetaCompraResponse>> GetAllAsync(int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);

        var metas = await _repository.GetAllAsync();
        return metas.Select(MapToResponse).ToList();
    }

    public async Task<List<EmpresaMetaResponse>> GetEmpresasAsync(int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);
        return EmpresasFixas.ToList();
    }

    public async Task<MetaCompraResponse> CreateAsync(MetaCompraRequest request, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);
        ValidarRequest(request);

        var meta = new MetaCompra
        {
            CdLinha = request.CdLinha,
            Safra = request.Safra!,
            DtInicial = request.DtInicial!.Value,
            DtFinal = request.DtFinal!.Value,
            MetaQtde = request.MetaQtde!.Value,
            CdEmpresa = request.CdEmpresa!.Value
        };

        var idGerado = await _repository.InsertAsync(meta);
        meta.IdMetaCompra = idGerado;

        return MapToResponse(meta);
    }

    public async Task<bool> UpdateAsync(long id, MetaCompraRequest request, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);
        ValidarRequest(request);

        var existente = await _repository.GetByIdAsync(id);
        if (existente == null) throw new KeyNotFoundException("Meta não encontrada.");

        existente.CdLinha = request.CdLinha;
        existente.Safra = request.Safra!;
        existente.DtInicial = request.DtInicial!.Value;
        existente.DtFinal = request.DtFinal!.Value;
        existente.MetaQtde = request.MetaQtde!.Value;
        existente.CdEmpresa = request.CdEmpresa!.Value;

        return await _repository.UpdateAsync(existente);
    }

    public async Task<bool> DeleteAsync(long id, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);

        var existente = await _repository.GetByIdAsync(id);
        if (existente == null) throw new KeyNotFoundException("Meta não encontrada.");

        Log.Information("Exclusão de meta: {@Meta}, Perfil: {Perfil}, Timestamp: {Timestamp}",
            existente, idPerfil, DateTime.Now);

        return await _repository.DeleteAsync(id);
    }

    private async Task VerificarPermissaoAsync(int idPerfil)
    {
        var pagina = await _acessoRepository.GetPaginaByChaveAsync("cadastroSafraMeta", idPerfil);
        if (pagina == null)
            throw new UnauthorizedAccessException("Sem permissão para acessar Cad. Safra/Meta.");
    }

    private static void ValidarRequest(MetaCompraRequest request)
    {
        if (request.DtInicial.HasValue && request.DtFinal.HasValue && request.DtFinal.Value < request.DtInicial.Value)
            throw new InvalidOperationException("A Data Final deve ser maior ou igual à Data Inicial.");

        if (request.CdEmpresa.HasValue && !EmpresasFixas.Any(e => e.CdEmpresa == request.CdEmpresa.Value))
            throw new InvalidOperationException("Empresa inválida.");
    }

    private static MetaCompraResponse MapToResponse(MetaCompra meta)
    {
        return new MetaCompraResponse
        {
            IdMetaCompra = meta.IdMetaCompra,
            CdLinha = meta.CdLinha,
            Safra = meta.Safra,
            DtInicial = meta.DtInicial.ToString("yyyy-MM-dd"),
            DtFinal = meta.DtFinal.ToString("yyyy-MM-dd"),
            MetaQtde = meta.MetaQtde,
            CdEmpresa = meta.CdEmpresa
        };
    }
}

// --- Agricola (Compras Frutas) ---
public interface IAgricolaService
{
    Task<GridDinamicaResponse> GetRecebimentoFrutasAsync(DateTime? data, int idPerfil);
    Task<GridDinamicaResponse> GetDetalhamentoAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, int idPerfil);
    Task<GridDinamicaResponse> GetNotasFiscaisAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, string? colunaGrauClicada, string? variedade, int idPerfil);
}

public class AgricolaService : IAgricolaService
{
    private readonly IAgricolaRepository _repository;
    private readonly IAcessoRepository _acessoRepository;

    private static readonly HashSet<string> ColunasClicadasValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "ANTERIOR",       // alias legado
        "DIA_ANTERIOR",   // coluna enviada pelo painel
        "QTDE_D0",
        "QTDE_D1",
        "QTDE_D2",
        "QTDE_D3",
        "QTDE_D4",
        "ACUMULADO",
    };

    public AgricolaService(IAgricolaRepository repository, IAcessoRepository acessoRepository)
    {
        _repository = repository;
        _acessoRepository = acessoRepository;
    }

    public async Task<GridDinamicaResponse> GetRecebimentoFrutasAsync(DateTime? data, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);

        var dataRef = data ?? DateTime.Today;
        var (colunas, linhas) = await _repository.GetRecebimentoFrutasAsync(dataRef);

        return new GridDinamicaResponse
        {
            DataReferencia = dataRef.ToString("yyyy-MM-dd"),
            UltimaAtualizacao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Colunas = colunas,
            Linhas = linhas
        };
    }

    public async Task<GridDinamicaResponse> GetDetalhamentoAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);
        ValidarColunaClicada(colunaClicada);

        var (colunas, linhas) = await _repository.GetRecebimentoFrutasDetAsync(data, empresa, linha, uf, colunaClicada);

        return new GridDinamicaResponse
        {
            DataReferencia = data.ToString("yyyy-MM-dd"),
            UltimaAtualizacao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Colunas = colunas,
            Linhas = linhas
        };
    }

    public async Task<GridDinamicaResponse> GetNotasFiscaisAsync(DateTime data, string empresa, string linha, string uf, string colunaClicada, string? colunaGrauClicada, string? variedade, int idPerfil)
    {
        await VerificarPermissaoAsync(idPerfil);
        ValidarColunaClicada(colunaClicada);

        var cdVariedade = CalcularCdVariedade(variedade);

        var (colunas, linhas) = await _repository.GetRecebimentoFrutasDetNfAsync(
            data, empresa, linha, uf, colunaClicada, colunaGrauClicada, cdVariedade);

        return new GridDinamicaResponse
        {
            DataReferencia = data.ToString("yyyy-MM-dd"),
            UltimaAtualizacao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Colunas = colunas,
            Linhas = linhas
        };
    }

    private async Task VerificarPermissaoAsync(int idPerfil)
    {
        var pagina = await _acessoRepository.GetPaginaByChaveAsync("comprasFrutasPorEmpresas", idPerfil);
        if (pagina == null)
            throw new UnauthorizedAccessException("Sem permissão para acessar Compras Frutas.");
    }

    private static void ValidarColunaClicada(string colunaClicada)
    {
        if (!ColunasClicadasValidas.Contains(colunaClicada))
            throw new ArgumentException($"Coluna clicada inválida: {colunaClicada}. Valores aceitos: {string.Join(", ", ColunasClicadasValidas)}");
    }

    private static string? CalcularCdVariedade(string? variedade)
    {
        if (string.IsNullOrEmpty(variedade))
            return null;

        var prefixo = variedade.Length >= 2 ? variedade.Substring(0, 2) : variedade;
        if (prefixo.StartsWith("TO"))
            return null;

        return prefixo;
    }
}

// --- Calendário (mantido) ---
public interface ICalendarioService
{
    Task<IEnumerable<CalendarioFinanceiro>> GetDatasAsync(int empresa, int ano, int mes);
    Task<bool> UpdateDiaUtilAsync(DateTime data, bool util);
}

public class CalendarioService : ICalendarioService
{
    private readonly ICalendarioRepository _repository;
    public CalendarioService(ICalendarioRepository repository) => _repository = repository;
    public Task<IEnumerable<CalendarioFinanceiro>> GetDatasAsync(int empresa, int ano, int mes)
        => _repository.GetDatasAsync(empresa, ano, mes);
    public Task<bool> UpdateDiaUtilAsync(DateTime data, bool util)
        => _repository.UpdateDiaUtilAsync(data, util);
}

// --- Agrupamento (mantido) ---
public interface IAgrupamentoService
{
    Task<IEnumerable<AgrupamentoDre>> GetAllAsync();
    Task<int> CreateAsync(AgrupamentoDre item);
    Task<bool> DeleteAsync(int id);
}

public class AgrupamentoService : IAgrupamentoService
{
    private readonly IAgrupamentoRepository _repository;
    public AgrupamentoService(IAgrupamentoRepository repository) => _repository = repository;
    public Task<IEnumerable<AgrupamentoDre>> GetAllAsync() => _repository.GetAllAsync();
    public Task<int> CreateAsync(AgrupamentoDre item) => _repository.CreateAsync(item);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}

// --- DBA (mantido) ---
public interface IDbaService
{
    Task<IEnumerable<SessaoOracle>> GetSessoesAsync();
    Task<bool> MatarSessaoAsync(int sid, int serial);
    Task<IEnumerable<TabelaLock>> GetLocksAsync();
}

public class DbaService : IDbaService
{
    private readonly IDbaRepository _repository;
    public DbaService(IDbaRepository repository) => _repository = repository;
    public Task<IEnumerable<SessaoOracle>> GetSessoesAsync() => _repository.GetSessoesAsync();
    public Task<bool> MatarSessaoAsync(int sid, int serial) => _repository.MatarSessaoAsync(sid, serial);
    public Task<IEnumerable<TabelaLock>> GetLocksAsync() => _repository.GetLocksAsync();
}
