using back_end.src.Domain.Coleta;
using back_end.src.Domain.MetalPesado;
using back_end.src.Medicoes.Application;

namespace Application.DTOs.Coleta;

// Extensão do cadastro: coleta, medições e metais são gravados no mesmo SaveChanges.
public record ColetaCadastroInput(
    int CorpoHidricoId,
    DateTimeOffset DataHora,
    double? Latitude = null,
    double? Longitude = null,
    double? ProfundidadeMetros = null,
    IReadOnlyCollection<InputMedicao>? Medicoes = null,
    IReadOnlyCollection<MetalMedidoInput>? MetaisPesados = null)
    : ColetaInput(CorpoHidricoId, DataHora, Latitude, Longitude, ProfundidadeMetros, Medicoes)
{
    public new ColetaEntity ToEntity()
    {
        if (DataHora == default || DataHora > DateTimeOffset.UtcNow)
            throw new ArgumentException("Informe uma data e hora de coleta válida e não futura.");
        var coleta = base.ToEntity();
        var simbolos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var metal in MetaisPesados ?? [])
        {
            if (metal is null || !simbolos.Add(metal.Nome))
                throw new ArgumentException("Informe cada metal apenas uma vez.");
            var unidade = metal.Nome switch
            {
                "Fe" or "Mn" => "mg/L",
                "Cr" or "Ni" or "Cu" or "Zn" or "Cd" or "Pb" => "µg/L",
                _ => throw new ArgumentException("Metal não suportado.")
            };
            if (metal.Unidade != unidade)
                throw new ArgumentException($"Use {unidade} para {metal.Nome}.");
            if (metal.Concentracao is null)
                throw new ArgumentException($"Informe a concentração de {metal.Nome}.");
            coleta.MetaisPesados.Add(new MetalPesadoEntity(metal.Nome, metal.Concentracao.Value, unidade, coleta));
        }
        return coleta;
    }
}

public record MetalMedidoInput(string Nome, double? Concentracao, string Unidade);
