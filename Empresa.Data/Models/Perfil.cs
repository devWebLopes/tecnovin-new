namespace Empresa.Data.Models;

/// <summary>
/// Representa um perfil de acesso do sistema — mapeia ACESSO_CADASTRO_PERFIL
/// </summary>
public class Perfil
{
    public int IDPerfil { get; set; }
    public string Descricao { get; set; } = string.Empty;
}