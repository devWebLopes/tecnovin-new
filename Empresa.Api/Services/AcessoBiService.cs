using Empresa.Api.DTOs.Request;
using Empresa.Api.DTOs.Response;
using Empresa.Api.Middleware;
using Empresa.Data.Models;
using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

/// <summary>
/// Serviço de Acesso B.I. — aplica o gate de permissão (RF01) e as regras de negócio
/// para gerenciar vínculos usuário×empresa em USUARIO_EMPRESA.
/// </summary>
public class AcessoBiService : IAcessoBiService
{
    private const string ChavePermissao = "cadastroUsuarioBi";

    private readonly IAcessoBiRepository _repository;
    private readonly IAcessoRepository _acessoRepository;
    private readonly ILogger<AcessoBiService> _logger;

    public AcessoBiService(
        IAcessoBiRepository repository,
        IAcessoRepository acessoRepository,
        ILogger<AcessoBiService> logger)
    {
        _repository = repository;
        _acessoRepository = acessoRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioBiResponse>> GetUsuariosComAcessoAsync(int idPerfil, string? search = null)
    {
        await VerificarPermissaoAsync(idPerfil);

        var usuarios = await _repository.GetUsuariosComAcessoAsync(search);
        return usuarios.Select(u => new UsuarioBiResponse
        {
            IdUsuario = u.IdUsuario,
            Nome = u.Nome,
            Empresas = u.Empresas
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmpresaBiResponse>> GetEmpresasDoUsuarioAsync(int idPerfil, long idUsuario)
    {
        await VerificarPermissaoAsync(idPerfil);

        var empresas = await _repository.GetEmpresasDoUsuarioAsync(idUsuario);
        return empresas.Select(MapToEmpresaBiResponse);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmpresaDisponivelResponse>> GetEmpresasDisponiveisAsync(int idPerfil, long idUsuario)
    {
        await VerificarPermissaoAsync(idPerfil);

        var empresas = await _repository.GetEmpresasDisponiveisAsync(idUsuario);
        return empresas.Select(e => new EmpresaDisponivelResponse
        {
            CodigoEmpresa = e.CdEmpresa,
            NomeFantasia = e.NmFantasia
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UsuarioBiResponse>> ConcederAcessoTotalAsync(int idPerfil, ConcederAcessoTotalRequest request)
    {
        await VerificarPermissaoAsync(idPerfil);

        // RF08.1 — validar que o usuário existe
        if (!await _repository.UsuarioExisteAsync(request.IdUsuario))
            throw new NotFoundException("Usuário", request.IdUsuario);

        // RF04.4 — correção D6: impede concessão total para usuário que já possui acesso
        if (await _repository.UsuarioPossuiAcessoAsync(request.IdUsuario))
        {
            _logger.LogWarning("Concessão total recusada — usuário {Id} já possui acesso ao B.I.", request.IdUsuario);
            throw new ConflictException("Usuário já possui acesso ao B.I. Conceda acesso por empresa na edição.");
        }

        await _repository.ConcederAcessoTotalAsync(request.IdUsuario);
        _logger.LogInformation("Acesso total ao B.I. concedido para o usuário {Id}", request.IdUsuario);

        // RF04.7 — retorna a lista atualizada
        return await GetUsuariosComAcessoAsync(idPerfil);
    }

    /// <inheritdoc />
    public async Task<EmpresaBiResponse> VincularEmpresaAsync(int idPerfil, long idUsuario, VincularEmpresaRequest request)
    {
        await VerificarPermissaoAsync(idPerfil);

        // RF08.1 — validar que o usuário existe
        if (!await _repository.UsuarioExisteAsync(idUsuario))
            throw new NotFoundException("Usuário", idUsuario);

        // RF08.2 — validar que a empresa existe
        if (!await _repository.EmpresaExisteAsync(request.CodigoEmpresa))
            throw new NotFoundException("Empresa", request.CodigoEmpresa);

        // RF05.6 / RF08.3 — prevenir duplicidade (ID_USUARIO, EMPRESA)
        if (await _repository.VinculoJaExisteAsync(idUsuario, request.CodigoEmpresa))
            throw new ConflictException("Vínculo entre usuário e empresa já existe.");

        await _repository.ConcederAcessoEmpresaAsync(idUsuario, request.CodigoEmpresa);
        _logger.LogInformation("Empresa {Empresa} vinculada ao usuário {Usuario} para acesso ao B.I.",
            request.CodigoEmpresa, idUsuario);

        // Retorna o vínculo criado (último registro do usuário para essa empresa)
        var empresas = await _repository.GetEmpresasDoUsuarioAsync(idUsuario);
        var vinculo = empresas.Last(e => e.Empresa == request.CodigoEmpresa);
        return MapToEmpresaBiResponse(vinculo);
    }

    /// <inheritdoc />
    public async Task RemoverAcessoAsync(int idPerfil, long idUsuarioEmpresa)
    {
        await VerificarPermissaoAsync(idPerfil);

        // RF06.5 — validar existência antes de remover
        if (!await _repository.VinculoExisteAsync(idUsuarioEmpresa))
            throw new NotFoundException("Vínculo de acesso ao B.I.", idUsuarioEmpresa);

        // RF06.2/RF06.3 — correção D2: remove pela PK real da linha detalhe
        await _repository.RemoverAcessoAsync(idUsuarioEmpresa);
        _logger.LogInformation("Vínculo {Id} de acesso ao B.I. removido", idUsuarioEmpresa);
    }

    /// <summary>
    /// RF01 — Gate de permissão: verifica se o perfil possui a página de chave cadastroUsuarioBi
    /// </summary>
    private async Task VerificarPermissaoAsync(int idPerfil)
    {
        var pagina = await _acessoRepository.GetPaginaByChaveAsync(ChavePermissao, idPerfil);
        if (pagina is null)
        {
            _logger.LogWarning("Acesso negado ao painel Acesso B.I. — perfil {Perfil} sem permissão", idPerfil);
            throw new UnauthorizedAccessException("Sem permissão de acesso a esta página.");
        }
    }

    private static EmpresaBiResponse MapToEmpresaBiResponse(UsuarioEmpresa empresa)
    {
        return new EmpresaBiResponse
        {
            IdUsuarioEmpresa = empresa.IdUsuarioEmpresa,
            IdUsuario = empresa.IdUsuario,
            CodigoEmpresa = empresa.Empresa,
            NomeFantasia = empresa.NomeFantasia
        };
    }
}