using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace back_end.src.Relatorios;

public sealed class ReportPdfDocument
{
    private const string Primary = "#17A6BF", Ink = "#222222", Muted = "#5C5C5C", Line = "#DADADA";
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public byte[] Generate(ReportDocument report)
    {
        var data = report.Data;
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(32); page.DefaultTextStyle(style => style.FontSize(9).FontColor(Ink));
            page.Header().Repeat().PaddingBottom(14).BorderBottom(1).BorderColor(Line).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("WaterPath").Bold().FontSize(21).FontColor(Primary);
                    column.Item().Text("MONITORAMENTO AMBIENTAL").FontSize(7).FontColor(Muted);
                    column.Item().PaddingTop(10).Text("RELATÓRIO DE MONITORAMENTO").Bold().FontSize(15);
                    column.Item().PaddingTop(3).Text(data.WaterBodyName).SemiBold().FontSize(11);
                    column.Item().Text(data.Location).FontColor(Muted);
                });
                row.ConstantItem(145).AlignRight().Column(column =>
                {
                    column.Item().Text($"REL. {data.Id.ToString("N")[..10].ToUpperInvariant()}").SemiBold().FontSize(9);
                    column.Item().Text($"{data.Config.StartDate:dd/MM/yyyy} a {data.Config.EndDate:dd/MM/yyyy}").FontSize(8);
                    column.Item().PaddingTop(4).Text($"Gerado: {ReportTime.Format(data.GeneratedAt)}").FontSize(8).FontColor(Muted);
                    column.Item().Text($"Por: {data.IssuedBy}").FontSize(8).FontColor(Muted);
                });
            });
            page.Content().PaddingTop(14).Column(column =>
            {
                column.Spacing(14);
                var latest = data.Collections[0];
                column.Item().Background(RiskBackground(data, latest.Risk?.Level)).Padding(12).Column(card =>
                {
                    card.Item().Text("RISCO DA ÚLTIMA COLETA").FontSize(8).SemiBold().FontColor(Muted);
                    card.Item().PaddingTop(3).Text(ReportComposer.Risk(latest)).FontSize(17).Bold();
                    card.Item().PaddingTop(3).Text($"Coleta #{latest.Id} - {ReportTime.Format(latest.CollectedAt)} (Brasília)").FontSize(8);
                });
                var number = 0;
                foreach (var block in report.Blocks)
                {
                    var currentNumber = ++number;
                    column.Item().EnsureSpace(65).Column(section =>
                    {
                        section.Spacing(5);
                        section.Item().BorderBottom(0.6f).BorderColor(Line).PaddingBottom(5)
                            .Text($"{currentNumber:00}   {block.Title.ToUpperInvariant()}").FontSize(10).Bold().FontColor(Primary);
                        foreach (var paragraph in block.Paragraphs)
                        {
                            var container = section.Item();
                            if (block.Key == "alerts" && data.Config.AdditionalSettings.HighlightCriticalPoints)
                                container = container.Background("#FFF6E8").Padding(6);
                            container.Text(paragraph).FontSize(block.Key == "method" ? 8 : 9).LineHeight(1.25f);
                        }
                        if (block.Key == "history")
                        {
                            foreach (var code in new[] { 1, 4 })
                            {
                                var svg = Chart(data, code);
                                if (svg is not null) section.Item().PaddingVertical(5).Height(145).Svg(svg);
                            }
                        }
                        if (block.Table is { Rows.Length: > 0 } table) section.Item().PaddingTop(3).Element(item => Table(item, table));
                        if (block.Key == "visual")
                            foreach (var image in data.Images)
                            {
                                section.Item().EnsureSpace(160).PaddingTop(8).Column(images =>
                                {
                                    images.Item().Text($"Coleta #{image.CollectionId} - análise #{image.PredictionId}").SemiBold().FontSize(8);
                                    images.Item().PaddingTop(4).Row(row =>
                                    {
                                        row.RelativeItem().PaddingRight(5).Column(c => { c.Item().Text("Imagem original").FontSize(8).FontColor(Muted); c.Item().Height(135).Element(item => Image(item, image.Original)); });
                                        row.RelativeItem().PaddingLeft(5).Column(c => { c.Item().Text("Detecções da IA").FontSize(8).FontColor(Muted); c.Item().Height(135).Element(item => Image(item, image.Annotated)); });
                                    });
                                });
                            }
                    });
                }
            });
            page.Footer().PaddingTop(12).BorderTop(0.6f).BorderColor(Line).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("WaterPath - Apoio ao monitoramento. Não constitui laudo de potabilidade.").FontSize(7).FontColor(Muted);
                row.ConstantItem(70).AlignRight().Text(text => { text.DefaultTextStyle(s => s.FontSize(7)); text.Span("Página "); text.CurrentPageNumber(); text.Span(" de "); text.TotalPages(); });
            });
        })).GeneratePdf();
    }

    private static string RiskBackground(ReportData data, int? level) => !data.Config.AdditionalSettings.IncludeVisualAlerts
        ? "#F3F4F5" : level switch { 1 => "#EFF8F1", 2 => "#FFF6E8", 3 => "#FFF0EA", _ => "#F3F4F5" };

    private static void Table(IContainer container, ReportTable table) => container.Table(t =>
    {
        t.ColumnsDefinition(columns => { foreach (var _ in table.Headers) columns.RelativeColumn(); });
        t.Header(header => { foreach (var title in table.Headers) header.Cell().Background("#EAF7FA").Padding(6).Text(title).Bold().FontSize(8); });
        foreach (var row in table.Rows)
            foreach (var value in row)
                t.Cell().BorderBottom(0.5f).BorderColor(Line).Padding(6).Text(value).FontSize(8).WrapAnywhere();
    });

    private static void Image(IContainer container, byte[] bytes)
    {
        try
        {
            var image = QuestPDF.Infrastructure.Image.FromBinaryData(bytes);
            container.Image(image).FitArea();
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        { container.Background("#F3F4F5").AlignCenter().AlignMiddle().Text("Imagem indisponível").FontColor(Muted).FontSize(9); }
    }

    // Datas reais no eixo X; lacunas, censura e unidades incompatíveis quebram os segmentos.
    public static string? Chart(ReportData data, int code)
    {
        var latest = data.Collections[0];
        var context = data.Collections.Where(c => c.Latitude == latest.Latitude && c.Longitude == latest.Longitude && c.Depth == latest.Depth)
            .OrderBy(c => c.CollectedAt).ThenBy(c => c.Id).ToArray();
        var measurements = context.Select(c => c.Measurements.FirstOrDefault(m => m.Code == code)).ToArray();
        var unit = measurements.LastOrDefault(m => m is not null)?.Unit;
        var points = measurements.Select((m, i) => new { Time = context[i].CollectedAt.ToUnixTimeMilliseconds(), Value = m is { Censored: false } && m.Unit == unit ? m.Value : null }).ToArray();
        var valid = points.Where(p => p.Value.HasValue).ToArray();
        if (valid.Length == 0) return null;
        var min = valid.Min(p => p.Value!.Value); var max = valid.Max(p => p.Value!.Value);
        var pad = Math.Max(0.1, (max - min) * 0.2); min -= pad; max += pad;
        var start = points[0].Time; var end = points[^1].Time;
        double X(long t) => start == end ? 260 : 55 + (t - start) / (double)(end - start) * 430;
        double Y(double v) => 115 - (v - min) / (max - min) * 72;
        static string N(double n) => n.ToString("0.##", Invariant);
        static string E(string s) => System.Security.SecurityElement.Escape(s) ?? "";
        var svg = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' width='520' height='160' viewBox='0 0 520 160'><rect width='520' height='160' fill='#FAFCFD'/><text x='12' y='20' font-family='Arial' font-size='11' fill='{Ink}'>{E(ReportParameters.All.Single(p => p.Code == code).Label)} ({E(unit ?? "")})</text>");
        foreach (var tick in new[] { min, (min + max) / 2, max })
            svg.Append($"<line x1='55' x2='485' y1='{N(Y(tick))}' y2='{N(Y(tick))}' stroke='#E1E7EA'/><text x='48' y='{N(Y(tick) + 3)}' text-anchor='end' font-family='Arial' font-size='9' fill='{Muted}'>{E(NumberForChart(tick))}</text>");
        for (var i = 0; i < points.Length; i++)
        {
            var p = points[i]; if (!p.Value.HasValue) continue;
            if (i > 0 && points[i - 1].Value is { } previous)
                svg.Append($"<line x1='{N(X(points[i-1].Time))}' x2='{N(X(p.Time))}' y1='{N(Y(previous))}' y2='{N(Y(p.Value.Value))}' stroke='{Primary}' stroke-width='2'/>");
            svg.Append($"<circle cx='{N(X(p.Time))}' cy='{N(Y(p.Value.Value))}' r='3' fill='{Primary}'/>");
        }
        svg.Append($"<text x='55' y='142' font-family='Arial' font-size='9' fill='{Muted}'>{E(ReportTime.Format(context[0].CollectedAt))}</text><text x='485' y='142' text-anchor='end' font-family='Arial' font-size='9' fill='{Muted}'>{E(ReportTime.Format(context[^1].CollectedAt))}</text></svg>");
        return svg.ToString();
    }
    private static string NumberForChart(double number) => number.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"));
}
