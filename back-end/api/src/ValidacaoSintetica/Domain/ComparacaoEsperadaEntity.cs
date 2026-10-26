using back_end.src.Domain.Coleta;

namespace back_end.src.ValidacaoSintetica.Domain;

/// <summary>A known sample value kept separate from any value produced by inference.</summary>
public sealed class ComparacaoEsperadaEntity
{
    public int Id { get; set; }
    public int ColetaId { get; set; }
    public ColetaEntity Coleta { get; set; } = null!;
    public string AmostraId { get; set; } = null!;
    public string Parametro { get; set; } = null!;
    public double ValorEsperado { get; set; }
    public double? ValorPrevisto { get; set; }
    public string Unidade { get; set; } = null!;
    public string Origem { get; set; } = null!;
    public bool Sintetico { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
