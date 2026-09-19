namespace Empresa.Api.DTOs;

/// <summary>
/// DTO de resposta com dados do estabelecimento
/// </summary>
public class EstabelecimentoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public bool Ativo { get; set; }
}