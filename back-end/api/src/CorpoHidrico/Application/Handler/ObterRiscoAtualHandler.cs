using System.Text.Json;
using Application.DTOs;
using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA.Application;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterRiscoAtualHandler(ICorpoHidricoRepository repository)
    : IRequestHandler<QueryObterRiscoAtual, RiscoAtualDTO?>
{
    public async Task<RiscoAtualDTO?> Handle(QueryObterRiscoAtual request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0) throw new ArgumentException("Informe um ID de corpo hídrico válido.");
        cancellationToken.ThrowIfCancellationRequested();
        if (repository.ObterCorpoHidricoPorId(request.Id) is null)
            throw new KeyNotFoundException("Corpo hídrico não encontrado");

        await foreach (var result in repository.ObterResultadosRisco(request.Id).WithCancellation(cancellationToken))
        {
            // Ignora resultados antigos/incompletos; nunca transforma ausência em risco baixo.
            var risk = LerRisco(request.Id, result);
            if (risk is not null) return risk;
        }
        return null;
    }

    private static RiscoAtualDTO? LerRisco(int corpoHidricoId, ResultadoRiscoPersistido result)
    {
        try
        {
            if (!ContratoRisco.Valido(result.EntradaJson, result.ResultadoJson)) return null;
            using var document = JsonDocument.Parse(result.ResultadoJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("riskRuleVersion", out var version) || version.ValueKind != JsonValueKind.String
                || version.GetString() != PredicaoIAService.VersaoRegraRisco
                || !root.TryGetProperty("riskLevel", out var risk) || risk.ValueKind != JsonValueKind.Number
                || !risk.TryGetInt32(out var level) || level is < 1 or > 3
                || !root.TryGetProperty("baseRiskLevel", out var basis) || basis.ValueKind != JsonValueKind.Number
                || !basis.TryGetInt32(out var baseLevel) || baseLevel is < 1 or > 3
                || !root.TryGetProperty("riskReasons", out var reasons) || reasons.ValueKind != JsonValueKind.Array
                || !root.TryGetProperty("history", out var history) || history.ValueKind != JsonValueKind.Object)
                return null;

            var motivos = new List<string>();
            foreach (var reason in reasons.EnumerateArray())
            {
                if (reason.ValueKind != JsonValueKind.String) return null;
                motivos.Add(reason.GetString()!);
            }
            // O rótulo é opcional no contrato do IaClient; não inventa um se estiver ausente.
            var label = root.TryGetProperty("riskLabel", out var savedLabel)
                && savedLabel.ValueKind == JsonValueKind.String ? savedLabel.GetString() : null;
            return new RiscoAtualDTO(corpoHidricoId, level, label, motivos,
                result.ColetaId, result.DataColeta, result.PredicaoId, result.DataPredicao, version.GetString()!,
                root.TryGetProperty("metalVariation", out var variation) ? variation.Clone() : null);
        }
        catch (JsonException) { return null; }
    }
}
