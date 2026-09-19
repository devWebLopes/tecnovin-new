using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs;

/// <summary>
/// DTO para criação/atualização de usuário
/// </summary>
public class UsuarioRequest
{
    /// <summary>
    /// Nome do usuário (obrigatório)
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Login do usuário (obrigatório, único)
    /// </summary>
    [Required(ErrorMessage = "Login é obrigatório")]
    [StringLength(50, ErrorMessage = "Login deve ter no máximo 50 caracteres")]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário (obrigatório na criação, opcional no update, mínimo 6 caracteres)
    /// </summary>
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string? Senha { get; set; }

    /// <summary>
    /// ID do perfil (obrigatório)
    /// </summary>
    [Required(ErrorMessage = "Perfil é obrigatório")]
    public int IdPerfil { get; set; }

    /// <summary>
    /// Indica se o usuário está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica se o usuário precisa atualizar a senha no próximo login
    /// </summary>
    public bool AtualizaSenha { get; set; }
}