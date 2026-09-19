using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs.Request;

/// <summary>
/// DTO para criação/atualização de perfil
/// </summary>
public class PerfilRequest
{
    /// <summary>
    /// Descrição do perfil (obrigatório, max 255 caracteres)
    /// </summary>
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(255, ErrorMessage = "Descrição deve ter no máximo 255 caracteres")]
    public string Descricao { get; set; } = string.Empty;
}