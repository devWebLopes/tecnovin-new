namespace Empresa.Data.Models;

/// <summary>
/// Representa o vínculo entre usuário e estabelecimento — mapeia ACESSO_USUARIO_EMPRESA_ESTAB
/// </summary>
public class UsuarioEmpresaEstab
{
    public int IDUsuario { get; set; }
    public int CdEmpresa { get; set; }
    public int CdEstabelecimento { get; set; }
}