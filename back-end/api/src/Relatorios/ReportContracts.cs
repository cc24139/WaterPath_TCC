using System.Text.Json.Serialization;
using back_end.src.IA.Application;

namespace back_end.src.Relatorios;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReportSections(bool ExecutiveSummary, bool CurrentData, bool IndicatorEvolution,
    bool HeavyMetals, bool VisualEvidence, bool DetailedMethodology)
{
    [JsonIgnore] public bool Any => ExecutiveSummary || CurrentData || IndicatorEvolution || HeavyMetals || VisualEvidence || DetailedMethodology;
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReportSettings(bool HighlightCriticalPoints, bool IncludeVisualAlerts);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReportConfig(DateOnly StartDate, DateOnly EndDate, string DetailLevel,
    string DocumentLength, ReportSections IncludedSections, ReportSettings AdditionalSettings)
{
    public void Validate(DateOnly today)
    {
        if (StartDate.Year < 1900 || EndDate < StartDate || EndDate > today || EndDate.DayNumber - StartDate.DayNumber > 365)
            throw new IaException(400, "Informe um período válido de até 366 dias, sem datas futuras.");
        if (DetailLevel is not ("resumo" or "medio" or "tecnico") || DocumentLength is not ("curto" or "medio" or "completo")
            || IncludedSections is null || !IncludedSections.Any || AdditionalSettings is null)
            throw new IaException(400, "Revise o detalhamento, a extensão e as seções do relatório.");
    }
}
public sealed record ReportRequest(int CorpoHidricoId, ReportConfig Config);
public sealed record ReportMeasurement(int Code, string Name, double? Value, string Unit, bool Censored, double? Limit);
public sealed record ReportMetal(string Name, double Value, string Unit, string Source);
public sealed record ReportDetection(string Name, int Count);
public sealed record ReportVariation(string Name, double? Delta, string? Unit, string State, string? ReferenceDate, string Source);
public sealed record ReportCollection(int Id, DateTimeOffset CollectedAt, string? Responsible,
    double? Latitude, double? Longitude, double? Depth, ReportMeasurement[] Measurements, ReportMetal[] Metals,
    int? PredictionId, DateTime? AnalyzedAt, AnalysisRisk? Risk, ReportDetection[] Detections, ReportVariation[] Variations);
public sealed record ReportImage(int CollectionId, int PredictionId,
    [property: JsonIgnore] byte[] Original, [property: JsonIgnore] byte[] Annotated);
public sealed record ReportData(Guid Id, int WaterBodyId, string WaterBodyName, string Location,
    string IssuedBy, DateTimeOffset GeneratedAt, ReportConfig Config, bool RestrictedScope,
    ReportCollection[] Collections, ReportImage[] Images, string[] Notices);
public sealed record ReportTable(string[] Headers, string[][] Rows);
public sealed record ReportBlock(string Key, string Title, string[] Paragraphs, ReportTable? Table = null);
public sealed record ReportDocument(ReportData Data, ReportBlock[] Blocks);

public static class ReportTime
{
    public static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    public static string Format(DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, Zone).ToString("dd/MM/yyyy HH:mm");
    public static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Zone).DateTime);
    public static DateTimeOffset Start(DateOnly date) => new(TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), Zone));
}
