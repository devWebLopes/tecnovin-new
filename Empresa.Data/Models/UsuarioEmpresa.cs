namespace Empresa.Data.Models;

/// <summary>
/// Representa um vínculo usuário×empresa para acesso ao B.I.
/// Mapeia a tabela USUARIO_EMPRESA (N:N entre usuários e empresas).
/// </summary>
public class UsuarioEmpresa
{
    /// <summary>
    /// PK — gerada por trigger/sequence Oracle (nunca informada pela aplicação)
    /// </summary>
    public long IdUsuarioEmpresa { get; set; }

    /// <summary>
    /// FK → ACESSO_CADASTRO_USUARIO.ID_USUARIO
    /// </summary>
    public long IdUsuario { get; set; }

    /// <summary>
    /// FK → VW_EMPRESA_NEW.CD_EMPRESA
    /// </summary>
    public long Empresa { get; set; }

    /// <summary>
    /// Propriedade de navegação — NOME (JOIN com ACESSO_CADASTRO_USUARIO)
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Propriedade de navegação — NM_FANTASIA (JOIN com VW_EMPRESA_NEW)
    /// </summary>
    public string NomeFantasia { get; set; } = string.Empty;
}