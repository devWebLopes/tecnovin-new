using Empresa.Data.Repositories;

namespace Empresa.Api.Services;

/// <summary>
/// Contrato do serviço de auditoria de relatórios
/// </summary>
public interface IRelatorioAuditoriaService
{
    /// <summary>
    /// Registra a geração de um relatório em REGISTRO_RELATORIOS.
    /// Deve ser chamado em todo endpoint que retorna dados de relatório.
    /// </summary>
    /// <param name="idUsuario">ID do usuário autenticado (claim ID_USUARIO do JWT)</param>
    /// <param name="tipoRelatorio">Identificador do tipo de relatório (ex: "COMPRAS_RESUMO_ANUAL")</param>
    /// <param name="ip">IP do cliente (extraído do HttpContext)</param>
    Task RegistrarAsync(int idUsuario, string tipoRelatorio, string? ip = null);
}
