namespace Empresa.Data.Models;

/// <summary>
/// Página de menu com contador de acessos do usuário (top "Mais Acessados").
/// Herda de <see cref="Pagina"/> e acrescenta QUANTIDADE de ACESSO_VISUALIZACAO_PAGINA.
/// </summary>
public class PaginaAcesso : Pagina
{
    public int Quantidade { get; set; }
}
