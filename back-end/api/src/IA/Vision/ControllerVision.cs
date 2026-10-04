using System.ComponentModel.DataAnnotations;
using back_end.src.IA.Application;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Vision;

[ApiController]
[Route("api/vision")]
public class ControllerVision(PredicaoIAService service) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PredicaoIAService.LimiteImagem + 1024 * 1024)]
    public async Task<IActionResult> PredizerImagem([FromForm, Required] IFormFile file,
        [FromForm, Range(1, int.MaxValue)] int coletaId, CancellationToken cancellationToken)
    {
        try
        {
            var predicao = await service.CriarAsync(coletaId, file, null, cancellationToken);
            Response.Headers["X-Predicao-Id"] = predicao.Id.ToString();
            Response.Headers.Location = $"/api/ia/predicoes/{predicao.Id}";
            return File(predicao.ImagemResultado, predicao.ContentTypeResultado, "predicao.jpg");
        }
        catch (IaException ex) { return Problem(statusCode: ex.StatusCode, detail: ex.Message); }
    }
}
