using back_end.src.Domain.Coleta;
using back_end.src.Medicoes.Application;
using back_end.src.Medicoes.Application.Handler;

namespace Application.DTOs.Coleta;

public record ColetaInput(
    int CorpoHidricoId,
    DateTimeOffset DataHora,
    double? Latitude = null,
    double? Longitude = null,
    double? ProfundidadeMetros = null,
    IReadOnlyCollection<InputMedicao>? Medicoes = null)
{
    public ColetaEntity ToEntity()
    {
        var coleta = new ColetaEntity(CorpoHidricoId, DataHora.ToUniversalTime(),
            Latitude, Longitude, ProfundidadeMetros);
        foreach (var medicao in Medicoes ?? [])
            coleta.AdicionarMedicao(MedicaoHandlerSupport.CriarMedicao(medicao));
        return coleta;
    }
}
