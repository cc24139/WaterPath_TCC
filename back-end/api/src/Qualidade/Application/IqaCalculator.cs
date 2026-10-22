namespace back_end.src.Qualidade.Application;

/// <summary>
/// Applies the same IQA display bands used by the frontend.
/// This is classification only, not a numerical calculation from measurements:
/// the frontend consumes a stored IQA score and the project defines no score formula.
/// </summary>
public static class IqaCalculator
{
    public static string Classificar(int? iqa)
    {
        if (iqa is null || iqa < 0 || iqa > 100)
            return "Sem classificação";

        if (iqa >= 90)
            return "Ótima";
        if (iqa >= 75)
            return "Boa";
        if (iqa >= 50)
            return "Atenção";

        return "Crítica";
    }
}
