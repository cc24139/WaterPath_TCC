using System;
using Application.Commands.QualidadeFutura;
using Application.Queries.QualidadeFutura;
using back_end.src.Domain.QualidadeFutura;
using back_end.src.IA.Application;
using back_end.src.QualidadeFutura.Application;
using back_end.src.Medicoes.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace back_end.src.Controllers.QualidadeFutura
{
    [ApiController]
    [Route("api/qualidadefutura")]
    public class ControllerQualidadeFutura : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly PrevisaoQualidadeFuturaService previsoes;

        public ControllerQualidadeFutura(IMediator mediator, PrevisaoQualidadeFuturaService previsoes)
        {
            this.mediator = mediator;
            this.previsoes = previsoes;
        }

        [HttpPost("previsoes")]
        public async Task<IActionResult> Prever([FromBody] CriarPrevisaoInput input,
            CancellationToken cancellationToken)
        {
            try
            {
                var previsao = await previsoes.CriarAsync(input, cancellationToken);
                return Created($"api/qualidadefutura/previsoes/{previsao.Id}", Representar(previsao));
            }
            catch (IaException ex) { return StatusCode(ex.StatusCode, new { ex.Code, Message = ex.Message }); }
        }

        [HttpGet("previsoes/{id:int}")]
        public async Task<IActionResult> ObterPrevisao(int id, CancellationToken cancellationToken)
        {
            if (id <= 0) return BadRequest("Informe um ID válido.");
            var previsao = await previsoes.ObterPorIdAsync(id, cancellationToken);
            return previsao is null ? NotFound("Previsão não encontrada.") : Ok(Representar(previsao));
        }

        [HttpGet("previsoes")]
        public async Task<IActionResult> ListarPrevisoes([FromQuery] int corpoHidricoId,
            [FromQuery] int? horizonteDias, CancellationToken cancellationToken)
        {
            try
            {
                var lista = await previsoes.ListarAsync(corpoHidricoId, horizonteDias, cancellationToken);
                return Ok(lista.Select(Representar));
            }
            catch (IaException ex) { return StatusCode(ex.StatusCode, new { ex.Code, Message = ex.Message }); }
        }

        private static object Representar(QualidadeFuturaEntity previsao) => new
        {
            previsao.Id, previsao.CorpoHidricoId, previsao.DataReferencia,
            previsao.DataAlvo, previsao.HorizonteDias, previsao.Ponto,
            previsao.Profundidade, previsao.CriadaEm,
            Resultado = JsonDocument.Parse(previsao.ResultadoJson!).RootElement.Clone(),
        };

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(
                    new CommandCadastrarQualidadeFutura { QualidadeFutura = qualidadeFutura },
                    cancellationToken
                );
                return Created($"api/qualidadefutura/{qualidadeFutura.Id}", qualidadeFutura);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        {
            try
            {
                var qualidadeFutura = await mediator.Send(
                    new QueryObterQualidadeFuturaPorId { Id = id },
                    cancellationToken
                );
                if (qualidadeFutura == null)
                    return NotFound("Qualidade futura não encontrada");
                return Ok(qualidadeFutura);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}/medicoes/{parametro}")]
        public async Task<IActionResult> ObterMedicao(int id, string parametro, CancellationToken cancellationToken)
        {
            if (!MedicaoParameterParser.TryParse(parametro, out var codigo))
                return BadRequest("Parâmetro de medição inválido.");

            var medicao = await mediator.Send(
                new QueryObterMedicaoDaQualidadeFutura { Id = id, CodigoMedicao = codigo },
                cancellationToken
            );
            return medicao.Count == 0
                ? NotFound("Qualidade futura ou parâmetro não encontrado.")
                : Ok(medicao.Select(item => new
                {
                    item.Id,
                    item.codigoMedicao,
                    item.valor,
                    item.unidade,
                    item.censurado,
                    item.limite,
                }));
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
        {
            try
            {
                var qualidadesFuturas = await mediator.Send(new QueryObterTodasQualidadesFuturas(), cancellationToken);
                return Ok(qualidadesFuturas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] QualidadeFuturaEntity qualidadeFutura,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await mediator.Send(
                    new CommandAtualizarQualidadeFutura { Id = id, QualidadeFutura = qualidadeFutura },
                    cancellationToken
                );
                return Ok("Qualidade futura atualizada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new CommandDeletarQualidadeFutura { Id = id }, cancellationToken);
                return Ok("Qualidade futura deletada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
