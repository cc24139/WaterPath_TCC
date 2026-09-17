using System;
using System.Net.Http.Headers;
using Application.Commands.QualidadeFutura;
using Application.Queries.QualidadeFutura;
using back_end.src.Domain.QualidadeFutura;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Vision
{
    [ApiController]
    [Route("api/vision")]
    public class ControllerVision : ControllerBase
    {
        private readonly WaterPathDbContext context;
        private static readonly HttpClient client = new HttpClient();
        private readonly string urlFastAPI = "http://127.0.0.1:8000";

        public ControllerVision(WaterPathDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PredizerImagem(
            [FromForm] IFormFile file,
            CancellationToken cancellationToken
        )
        {
            try
            {
                if (file is null || file.Length <= 0)
                    return BadRequest("Imagem vazia!");

                using var formData = new MultipartFormDataContent();
                var fileContent = await LerImagemAsync(file, cancellationToken);
                formData.Add(fileContent, "file", file.FileName);

                using var response = await client.PostAsync(
                    $"{urlFastAPI}/vision/predict",
                    formData,
                    cancellationToken
                );

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync(cancellationToken);
                    return StatusCode((int)response.StatusCode, erro);
                }

                var imagemPredita = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                return File(imagemPredita, contentType, "predicao.jpg");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private static async Task<HttpContent> LerImagemAsync(
            IFormFile file,
            CancellationToken cancellationToken = default
        )
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);

            var fileContent = new ByteArrayContent(memoryStream.ToArray());

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType
            );

            return fileContent;
        }
    }
}
