namespace Empresa.Data.Models;

/// <summary>
/// Representa o vínculo entre perfil e página — mapeia ACESSO_PERFIL_PAGINA
/// </summary>
public class PerfilPagina
{
    public int IDPerfil { get; set; }
    public int IDPagina { get; set; }
}