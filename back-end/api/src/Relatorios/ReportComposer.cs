using System.Globalization;

namespace back_end.src.Relatorios;

public static class ReportComposer
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-BR");
    public static string Number(double? value) => value?.ToString("G6", Pt) ?? "Não informado";
    public static string Measurement(ReportMeasurement? m) => m is null ? "Não medido"
        : $"{(m.Censored ? "< " + Number(m.Limit) : Number(m.Value))} {m.Unit}";
    public static string Risk(ReportCollection c) => c.Risk?.Level switch
    { 1 => "Baixo - nível 1", 2 => "Moderado - nível 2", 3 => "Alto - nível 3", _ => c.PredictionId.HasValue ? "Risco indisponível" : "Sem análise integrada" };

    public static ReportDocument Compose(ReportData data)
    {
        var config = data.Config;
        var sections = config.IncludedSections;
        var latest = data.Collections[0];
        var valid = data.Collections.Where(c => c.Risk is not null).ToArray();
        var limit = config.DocumentLength == "curto" ? 5 : config.DocumentLength == "medio" ? 30 : 200;
        var history = data.Collections.Take(limit).ToArray();
        var blocks = new List<ReportBlock>();
        var counts = $"{data.Collections.Length} coletas no período; {valid.Length} com risco válido, {data.Collections.Count(c => !c.PredictionId.HasValue)} sem análise integrada e {data.Collections.Count(c => c.PredictionId.HasValue && c.Risk is null)} com risco indisponível.";
        blocks.Add(new("coverage", "Abrangência do relatório", [counts,
            $"Última coleta: #{latest.Id}, em {ReportTime.Format(latest.CollectedAt)} (Brasília). {Risk(latest)}.",
            $"Riscos válidos: {valid.Count(c => c.Risk!.Level == 1)} baixos, {valid.Count(c => c.Risk!.Level == 2)} moderados e {valid.Count(c => c.Risk!.Level == 3)} altos. Contagem por coleta, sem média dos níveis."]));
        if (sections.ExecutiveSummary)
            blocks.Add(new("summary", "Síntese do período", latest.Risk is null
                ? ["A última coleta ainda não possui classificação de risco válida. Não foi substituída pelo resultado de uma coleta anterior."]
                : latest.Risk.Reasons.Prepend("Motivos registrados na análise da última coleta:").ToArray()));
        var alerts = data.Collections.Where(c => c.Risk?.Level >= 2)
            .SelectMany(c => c.Risk!.Reasons.Select(reason => $"Coleta #{c.Id} ({ReportTime.Format(c.CollectedAt)}): {reason}")).ToArray();
        // Os motivos de atenção nunca desaparecem por escolher um documento compacto ou desmarcar a síntese.
        if (alerts.Length > 0) blocks.Add(new("alerts", "Motivos de atenção registrados", alerts));
        if (sections.CurrentData)
        {
            var rows = ReportParameters.All.Select(p => new[] { p.Label, Measurement(latest.Measurements.FirstOrDefault(m => m.Code == p.Code)),
                latest.Measurements.Any(m => m.Code == p.Code && m.Censored) ? "Abaixo do limite de detecção" : latest.Measurements.Any(m => m.Code == p.Code) ? "Medido" : "Não informado" }).ToArray();
            blocks.Add(new("current", "Indicadores da última coleta", [config.DetailLevel == "resumo"
                ? $"Coleta #{latest.Id}. Valores medidos e suas unidades."
                : $"Coleta #{latest.Id}; responsável: {latest.Responsible ?? "não informado"}. Ponto: {Number(latest.Latitude)}, {Number(latest.Longitude)}; profundidade: {Number(latest.Depth)} m."],
                new(["Indicador", "Valor", "Disponibilidade"], rows)));
        }
        if (sections.IndicatorEvolution)
            blocks.Add(new("history", "Evolução das medições e riscos", [$"Tabela com {history.Length} das {data.Collections.Length} coletas, das mais recentes para as mais antigas. Os totais e motivos de atenção consideram todo o período. Gráficos usam o contexto da última coleta; valores ausentes não são interpolados."],
                new(["Coleta / data", "pH", "Condutividade", "Oxigênio dissolvido", "Risco"], history.Select(c => new[] {
                    $"#{c.Id}\n{ReportTime.Format(c.CollectedAt)}", Measurement(c.Measurements.FirstOrDefault(m => m.Code == 1)),
                    Measurement(c.Measurements.FirstOrDefault(m => m.Code == 4)), Measurement(c.Measurements.FirstOrDefault(m => m.Code == 2)), Risk(c) }).ToArray())));
        if (sections.HeavyMetals)
        {
            var chosen = config.DocumentLength == "completo" ? data.Collections : config.DocumentLength == "medio" ? data.Collections.Take(5).ToArray() : [latest];
            var rows = chosen.SelectMany(c => c.Metals.Select(m =>
            {
                var variation = c.Variations.FirstOrDefault(v => v.Name == m.Name);
                var delta = variation?.Delta is { } d && variation.State != "indeterminada" ? $"{Number(d)} {variation.Unit}" : "Indeterminada";
                return new[] { $"#{c.Id} / {m.Name}", $"{Number(m.Value)} {m.Unit}", m.Source,
                    delta, variation is null ? "Sem referência" : $"{variation.ReferenceDate ?? "Sem data"}\n{variation.Source}" };
            })).ToArray();
            blocks.Add(new("metals", "Metais pesados", [$"Apresentação de até {chosen.Length} coletas mais recentes. Medições e estimativas têm origem identificada. Variações usam a referência retornada pela análise; ausência de informação não significa concentração zero.",
                "As referências de comparação podem ter origem em medições e modelo. Não são limites regulatórios nem uma conclusão de conformidade."],
                rows.Length == 0 ? null : new(["Coleta / metal", "Concentração", "Origem", "Variação", "Referência da variação"], rows)));
            if (rows.Length == 0) blocks.Add(new("metals-empty", "Disponibilidade de metais", ["Não há concentrações medidas ou estimadas disponíveis nas coletas apresentadas."]));
        }
        if (sections.VisualEvidence)
            blocks.Add(new("visual", "Evidências visuais", [data.Images.Length == 0 ? "Não há imagens de análises integradas válidas disponíveis para este relatório."
                : $"Imagens originais e anotadas das {data.Images.Length} análises selecionadas. As detecções descrevem sinais visuais na fotografia, sem quantificar a contaminação de todo o corpo hídrico."],
                new(["Coleta", "Classe detectada", "Quantidade"], data.Images.SelectMany(image => data.Collections.Single(c => c.Id == image.CollectionId).Detections.Select(d => new[] { $"#{image.CollectionId}", d.Name, d.Count.ToString(Pt) })).ToArray())));
        var method = new List<string>
        {
            "Documento de apoio ao monitoramento. A classificação de risco provém da análise integrada persistida: nível 1 baixo, 2 moderado e 3 alto. Não corresponde a IQA ou laudo de potabilidade.",
            "A regra de triagem usa sinais visuais, pH, oxigênio dissolvido e histórico de alertas. Metais estimados e suas variações são apresentados separadamente e não determinam diretamente esse nível de risco.",
            "Medições censuradas aparecem com '<' e não entram como valores exatos nos gráficos. Unidades são preservadas. As datas são apresentadas no horário de Brasília.",
        };
        if (sections.DetailedMethodology || config.DetailLevel == "tecnico")
        {
            method.Add("Uma predição integrada por coleta: a mais recente até a geração do documento, com desempate pelo ID. Resultados sem referência compatível ou com medições alteradas são sinalizados como indisponíveis.");
            method.Add("Comparações entre pontos ou profundidades diferentes não são inferidas pelo relatório. Os gráficos incluem apenas o ponto e a profundidade da última coleta e separam lacunas e unidades incompatíveis.");
            method.Add("Rastreabilidade: " + string.Join("; ", data.Collections.Select(c => $"coleta #{c.Id} / análise {(c.PredictionId.HasValue ? "#" + c.PredictionId : "ausente")}")) + ". Modelo de documento: waterpath-report-v1.");
        }
        method.AddRange(data.Notices);
        blocks.Add(new("method", "Metodologia e limites de interpretação", method.ToArray()));
        return new(data, blocks.ToArray());
    }
}
