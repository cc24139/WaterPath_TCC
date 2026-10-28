using System.Security.Claims;
using back_end.src.IA.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Relatorios;

[ApiController, Authorize, IaExceptionFilter, Route("api/relatorios")]
public sealed class ControllerRelatorios(ReportDataService service, ReportPdfDocument pdf) : ControllerBase
{
    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new IaException(401, "Entre novamente para gerar o relatório.");

    [HttpPost("pdf"), RequestSizeLimit(16384)]
    public async Task<IActionResult> Generate(ReportRequest request, CancellationToken ct)
    {
        var data = await service.BuildAsync(request, UserId(), User.Identity?.Name ?? "Usuário autenticado", ct);
        ct.ThrowIfCancellationRequested();
        var bytes = pdf.Generate(ReportComposer.Compose(data));
        Response.Headers.CacheControl = "no-store";
        return File(bytes, "application/pdf", $"waterpath-rio-{data.WaterBodyId}-{data.Config.EndDate:yyyyMMdd}.pdf");
    }
}
