using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs.Request;

/// <summary>
/// Concessão de acesso total ao B.I. (usuário → todas as empresas)
/// </summary>
public class ConcederAcessoTotalRequest
{
    /// <summary>
    /// ID do usuário em ACESSO_CADASTRO_USUARIO
    /// </summary>
    [Required(ErrorMessage = "IdUsuario é obrigatório")]
    public long IdUsuario { get; set; }
}

/// <summary>
/// Vinculação de uma empresa específica ao usuário
/// </summary>
public class VincularEmpresaRequest
{
    /// <summary>
    /// Código da empresa em VW_EMPRESA_NEW
    /// </summary>
    [Required(ErrorMessage = "CodigoEmpresa é obrigatório")]
    public long CodigoEmpresa { get; set; }
}