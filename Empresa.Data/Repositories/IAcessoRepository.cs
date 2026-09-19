using Empresa.Data.Models;

namespace Empresa.Data.Repositories;

/// <summary>
/// Interface do repositório de acesso (autenticação, menu, páginas)
/// </summary>
public interface IAcessoRepository
{
    /// <summary>
    /// Busca usuário por login e senha (hash) — valida ATIVO = 'S'
    /// </summary>
    Task<Usuario?> GetUsuarioAsync(string login, string senhaHash);

    /// <summary>
    /// Busca usuário por login (sem validação de senha)
    /// </summary>
    Task<Usuario?> GetUsuarioByLoginAsync(string login);

    /// <summary>
    /// Incrementa QUANTIDADE_ACESSO do usuário
    /// </summary>
    Task SalvaAcessoUsuarioAsync(int idUsuario);

    /// <summary>
    /// Retorna as páginas do menu hierárquico do perfil — CONNECT BY PRIOR reproduzindo o
    /// UNION legado: páginas permitidas + todos os ancestrais (pais implícitos, RN-03),
    /// profundidade ilimitada (D-07) e ATIVO='S' também nos ancestrais (D-06).
    /// </summary>
    Task<IEnumerable<Pagina>> GetPaginasMenuAsync(int idPerfil);

    /// <summary>
    /// Q10 — Verifica permissão de acesso a uma página por chave de controle + perfil do usuário
    /// </summary>
    Task<Pagina?> GetPaginaByChaveAsync(string chaveControle, int idPerfil);

    /// <summary>
    /// Retorna as TOP 10 páginas mais acessadas pelo usuário, filtradas pelo perfil (D-05)
    /// e ordenadas por QUANTIDADE DESC (D-04).
    /// </summary>
    Task<IEnumerable<PaginaAcesso>> GetPaginasMaisAcessadasAsync(int idUsuario, int idPerfil);

    /// <summary>
    /// Retorna os dados do usuário para a barra do menu (login, nome e contagem de
    /// vínculos em USUARIO_EMPRESA para o link do B.I., RN-09).
    /// </summary>
    Task<MenuUsuarioInfo?> GetMenuUsuarioAsync(int idUsuario);

    /// <summary>
    /// UPSERT em ACESSO_VISUALIZACAO_PAGINA — registra acesso do usuário a uma página
    /// </summary>
    Task RegistraAcessoPaginaAsync(int idUsuario, int idPagina);

    /// <summary>
    /// Retorna a árvore de estabelecimentos (GetNode/GetTree)
    /// </summary>
    Task<IEnumerable<Estabelecimento>> GetEstabelecimentosTreeAsync();

    /// <summary>
    /// INSERT em REGISTRO_RELATORIOS — registra geração de relatório (usuário, página, data/hora, tipo, IP)
    /// </summary>
    Task RegistraRelatorioAsync(int idUsuario, string tipoRelatorio, string? ip = null);
}