using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs.Request;

/// <summary>
/// Body do POST /api/v1/menu/acessos — registra telemetria de acesso (RF03).
/// </summary>
public class RegistroAcessoMenuRequest
{
    /// <summary>
    /// CHAVE_CONTROLE da página acessada — moeda de autorização (RN-12).
    /// </summary>
    [Required(ErrorMessage = "chaveControle é obrigatória.")]
    public string ChaveControle { get; set; } = string.Empty;
}
