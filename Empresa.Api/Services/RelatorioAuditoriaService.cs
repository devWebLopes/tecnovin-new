using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

/// <summary>
/// Serviço de auditoria de relatórios — registra geração em REGISTRO_RELATORIOS.
/// Registrado como Scoped para compartilhar o DbSession da requisição.
/// Falhas no registro nunca propagam exceção (fire-and-forget com log).
/// </summary>
public class RelatorioAuditoriaService : IRelatorioAuditoriaService
{
    private readonly IAcessoRepository _acessoRepository;
    private readonly ILogger<RelatorioAuditoriaService> _logger;

    public RelatorioAuditoriaService(
        IAcessoRepository acessoRepository,
        ILogger<RelatorioAuditoriaService> logger)
    {
        _acessoRepository = acessoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Registra a geração de um relatório em REGISTRO_RELATORIOS.
    /// Nunca lança exceção — falhas são apenas logadas para não interromper a resposta ao cliente.
    /// </summary>
    public async Task RegistrarAsync(int idUsuario, string tipoRelatorio, string? ip = null)
    {
        try
        {
            await _acessoRepository.RegistraRelatorioAsync(idUsuario, tipoRelatorio, ip);
            _logger.LogDebug("Relatório registrado: {Tipo} — usuário {IdUsuario}", tipoRelatorio, idUsuario);
        }
        catch (Exception ex)
        {
            // Nunca falha a requisição por erro de auditoria
            _logger.LogWarning(ex, "Falha ao registrar relatório {Tipo} para usuário {IdUsuario}", tipoRelatorio, idUsuario);
        }
    }
}
