namespace Empresa.Api.DTOs.Request;

/// <summary>
/// DTO para sincronizar vínculos de estabelecimentos de um usuário
/// </summary>
public class EstabelecimentoVinculoRequest
{
    /// <summary>
    /// Lista de estabelecimentos a vincular
    /// </summary>
    public List<VinculoEstabelecimentoItem> VincularEstabelecimentos { get; set; } = new();

    /// <summary>
    /// Lista de estabelecimentos a desvincular
    /// </summary>
    public List<VinculoEstabelecimentoItem> DesvincularEstabelecimentos { get; set; } = new();
}

/// <summary>
/// Item de vínculo de estabelecimento
/// </summary>
public class VinculoEstabelecimentoItem
{
    /// <summary>
    /// Código da empresa
    /// </summary>
    public int CdEmpresa { get; set; }

    /// <summary>
    /// Código do estabelecimento
    /// </summary>
    public int CdEstabelecimento { get; set; }
}