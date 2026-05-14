using System;
using Application.Commands.QualidadeFutura;
using Application.Queries.QualidadeFutura;
using back_end.src.Domain.QualidadeFutura;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Vision
{
    [ApiController]
    [Route("api/vision")]
    public class ControllerVision : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerVision(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> PredizerImagem([FromBody] string pathImagem)
        {
            try
            {
                // Implementation for predicting image
                return Ok("Imagem predita com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
