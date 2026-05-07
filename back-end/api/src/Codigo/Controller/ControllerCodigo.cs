using Application.Commands.Codigo;
using Application.Queries.Codigo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.Codigo
{
    [ApiController]
    [Route("api/codigo")]
    public class ControllerCodigo : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerCodigo(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("gerar")]
        public async Task<IActionResult> GerarCodigo([FromBody] CommandGerarCodigo command)
        {
            try
            {
                var codigoGerado = await mediator.Send(command);
                return Ok(new { codigo = codigoGerado });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("validar")]
        public async Task<IActionResult> ValidarCodigo([FromBody] CommandValidarCodigo command)
        {
            try
            {
                var resultado = await mediator.Send(command);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pendente/{usuarioId}")]
        public async Task<IActionResult> VerificarCodigoPendente(int usuarioId)
        {
            try
            {
                var query = new QueryCodigoPendente(usuarioId);
                var existePendente = await mediator.Send(query);
                return Ok(new { pendente = existePendente });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
