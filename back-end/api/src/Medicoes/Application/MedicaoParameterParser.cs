using System.Globalization;
using System.Text;
using back_end.src.Medicoes.Domain;

namespace back_end.src.Medicoes.Application;

public static class MedicaoParameterParser
{
    private static readonly IReadOnlyDictionary<string, Medicao> Aliases = new Dictionary<string, Medicao>
    {
        ["temp"] = Medicao.Temperatura,
        ["temperature"] = Medicao.Temperatura,
        ["ph"] = Medicao.Ph,
        ["potencialhidrogenico"] = Medicao.Ph,
        ["do"] = Medicao.OxigenioDissolvido,
        ["dissolvedoxygen"] = Medicao.OxigenioDissolvido,
        ["turbidity"] = Medicao.Turbidez,
        ["conductivity"] = Medicao.CondutividadeEletrica,
        ["electricalconductivity"] = Medicao.CondutividadeEletrica,
        ["tss"] = Medicao.SolidosSuspensosTotais,
        ["totalsuspendedsolids"] = Medicao.SolidosSuspensosTotais,
        ["color"] = Medicao.Cor,
        ["toc"] = Medicao.CarbonoOrganicoTotal,
        ["totalorganiccarbon"] = Medicao.CarbonoOrganicoTotal,
        ["bod5"] = Medicao.Dbo5,
        ["biochemicaloxygendemand"] = Medicao.Dbo5,
        ["phosphate"] = Medicao.Fosfato,
        ["totalphosphorus"] = Medicao.FosforoTotal,
        ["ammoniacalnitrogen"] = Medicao.NitrogenioAmoniacal,
        ["nitrite"] = Medicao.Nitrito,
        ["nitrate"] = Medicao.Nitrato,
        ["totalnitrogen"] = Medicao.NitrogenioTotal,
    };

    public static bool TryParse(string? parameter, out Medicao medicao)
    {
        var normalized = Normalize(parameter);
        if (Aliases.TryGetValue(normalized, out medicao))
            return true;

        foreach (var candidate in Enum.GetValues<Medicao>())
        {
            if (Normalize(candidate.ToString()) == normalized)
            {
                medicao = candidate;
                return true;
            }
        }

        medicao = default;
        return false;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
        }
        return builder.ToString();
    }
}
