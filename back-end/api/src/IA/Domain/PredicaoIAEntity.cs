using back_end.src.Domain.Coleta;

namespace back_end.src.IA.Domain;

public class PredicaoIAEntity
{
    public int Id { get; set; }
    public int ColetaId { get; set; }
    public ColetaEntity Coleta { get; set; } = null!;
    // Nulos somente em registros legados, cuja referência original não pode ser reconstruída.
    public int? CorpoHidricoId { get; set; }
    public DateTime? DataColeta { get; set; }
    public DateTime CriadaEm { get; set; }
    public string Tipo { get; set; } = null!;
    public string? EntradaJson { get; set; }
    public string? ResultadoJson { get; set; }
    public string NomeArquivo { get; set; } = null!;
    public string ContentTypeOriginal { get; set; } = null!;
    public byte[] ImagemOriginal { get; set; } = null!;
    public string ContentTypeResultado { get; set; } = null!;
    public byte[] ImagemResultado { get; set; } = null!;
}
