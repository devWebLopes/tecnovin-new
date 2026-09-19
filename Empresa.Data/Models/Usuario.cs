namespace Empresa.Data.Models;

/// <summary>
/// Representa um usuário do sistema — mapeia ACESSO_CADASTRO_USUARIO
/// </summary>
public class Usuario
{
    public int IDUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public int IDPerfil { get; set; }
    public string? DescricaoPerfil { get; set; }
    public int QuantidadeAcesso { get; set; }
    public string AtualizaSenha { get; set; } = "N";
    public string Senha { get; set; } = string.Empty;
    public string Ativo { get; set; } = "S";
}
