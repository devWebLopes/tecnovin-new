namespace Empresa.Data.Models;

/// <summary>
/// Representa um estabelecimento/empresa do sistema
/// </summary>
public class Estabelecimento
{
    public int IDEstabelecimento { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string Ativo { get; set; } = "S";
}