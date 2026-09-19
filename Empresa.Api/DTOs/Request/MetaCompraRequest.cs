using System.ComponentModel.DataAnnotations;

namespace Empresa.Api.DTOs.Request;

public class MetaCompraRequest
{
    [Required(ErrorMessage = "Informe uma Linha")]
    public long CdLinha { get; set; }

    [Required(ErrorMessage = "Informe uma Safra")]
    public string Safra { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a Data de Inicio da Safra")]
    public DateTime? DtInicial { get; set; }

    [Required(ErrorMessage = "Informe a Data final da Safra")]
    public DateTime? DtFinal { get; set; }

    [Required(ErrorMessage = "Informe a Meta a Ser Cadastrada")]
    public decimal? MetaQtde { get; set; }

    [Required(ErrorMessage = "Informe a Empresa")]
    public long? CdEmpresa { get; set; }
}
