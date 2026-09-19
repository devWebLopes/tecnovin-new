using Empresa.Api.DTOs.Response;

namespace Empresa.Api.Services;

/// <summary>
/// Serviço do menu — orquestra a árvore hierárquica (cacheada por perfil),
/// o painel "Mais Acessados" e a barra do usuário/B.I.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// RF01/RF02/RF05 — Retorna o shell completo do menu para o usuário autenticado.
    /// A árvore vem do cache (chave menu:tree:{perfil}), mais acessados e usuário sempre frescos.
    /// </summary>
    Task<MenuResponse> GetMenuAsync(int idUsuario, int idPerfil);

    /// <summary>
    /// RF03 — Registra a telemetria de acesso a uma página. Valida a permissão da chave
    /// (RN-12) ANTES do upsert — acesso não autorizado nunca é registrado.
    /// </summary>
    Task RegistrarAcessoAsync(string chaveControle, int idUsuario, int idPerfil);

    /// <summary>
    /// RF07 — Invalida o cache do menu. Sem idPerfil, invalida todos os perfis
    /// (usado ao criar/atualizar/excluir páginas).
    /// </summary>
    void Invalidate(int? idPerfil = null);
}
