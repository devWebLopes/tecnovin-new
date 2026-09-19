using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs.Request;

/// <summary>
/// DTO para sincronizar permissões de páginas de um perfil
/// </summary>
public class PerfilPaginasRequest
{
    /// <summary>
    /// IDs das páginas a vincular
    /// </summary>
    public List<int> VincularIds { get; set; } = new();

    /// <summary>
    /// IDs das páginas a desvincular
    /// </summary>
    public List<int> DesvincularIds { get; set; } = new();
}