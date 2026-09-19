namespace Empresa.Api.DTOs.Response;

/// <summary>
/// Linha do grid mestre — usuário com acesso ao B.I. e agregação das empresas
/// </summary>
public class UsuarioBiResponse
{
    public long IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Empresas { get; set; } = string.Empty;
}

/// <summary>
/// Linha do grid detalhe — empresa vinculada ao usuário
/// </summary>
public class EmpresaBiResponse
{
    public long IdUsuarioEmpresa { get; set; }
    public long IdUsuario { get; set; }
    public long CodigoEmpresa { get; set; }
    public string NomeFantasia { get; set; } = string.Empty;
}

/// <summary>
/// Empresa disponível para novo vínculo (dropdown do detalhe)
/// </summary>
public class EmpresaDisponivelResponse
{
    public long CodigoEmpresa { get; set; }
    public string NomeFantasia { get; set; } = string.Empty;
}