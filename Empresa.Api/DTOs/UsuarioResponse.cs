namespace Empresa.Api.DTOs;

/// <summary>
/// DTO de resposta com dados do usuário (sem senha)
/// </summary>
public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public int IdPerfil { get; set; }
    public string? DescricaoPerfil { get; set; }
    public int QuantidadeAcesso { get; set; }
    public bool AtualizaSenha { get; set; }
    public bool Ativo { get; set; }
    public DateTime? DataHoraUltimoAcesso { get; set; }
}