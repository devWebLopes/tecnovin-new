namespace Empresa.Data.Models;

/// <summary>
/// Dados do usuário exibidos na barra do menu (login, nome e vínculo B.I.).
/// Mapeia ACESSO_CADASTRO_USUARIO + contagem de USUARIO_EMPRESA.
/// </summary>
public class MenuUsuarioInfo
{
    public int IDUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de vínculos em USUARIO_EMPRESA — > 0 habilita o link do B.I. (RN-09)
    /// </summary>
    public int Vinculos { get; set; }
}
