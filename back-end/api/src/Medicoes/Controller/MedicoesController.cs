using back_end.src.Medicoes.Application;
using back_end.src.Medicoes.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Medicoes.Controller;

[ApiController]
[Route("api/medicoes")]
public class MedicoesController : ControllerBase
{
    private readonly IMediator mediator;

    public MedicoesController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public Task<IActionResult> Cadastrar([FromBody] CreateMedicaoCommand medicao, CancellationToken cancellationToken) =>
        Executar(async () =>
        {
            await mediator.Send(medicao, cancellationToken);
            return Ok("Medição criada com sucesso");
        });

    [HttpGet("{idColeta:int}/{idMedicao:int}")]
    public Task<IActionResult> ObterPorId(int idColeta, int idMedicao, CancellationToken cancellationToken) =>
        Executar(async () =>
        {
            var medicao = await mediator.Send(new QueryObterMedicaoPorId(idColeta, idMedicao), cancellationToken);
            return medicao is null ? NotFound("Medição não encontrada para essa coleta.") : Ok(medicao);
        });

    [HttpGet("{idColeta:int}")]
    public Task<IActionResult> ObterPorColeta(int idColeta, CancellationToken cancellationToken) =>
        Executar(async () => Ok(await mediator.Send(new QueryObterMedicoesPorColeta(idColeta), cancellationToken)));

    [HttpGet]
    public Task<IActionResult> ObterTodos(CancellationToken cancellationToken) =>
        Executar(async () => Ok(await mediator.Send(new QueryObterTodasMedicoes(), cancellationToken)));

    [HttpPut]
    public Task<IActionResult> Atualizar([FromBody] UpdateMedicaoCommand medicao, CancellationToken cancellationToken) =>
        Executar(async () =>
        {
            await mediator.Send(medicao, cancellationToken);
            return Ok("Medição atualizada com sucesso");
        });

    [HttpDelete("{idColeta:int}/{idMedicao:int}")]
    public Task<IActionResult> Deletar(int idColeta, int idMedicao, CancellationToken cancellationToken) =>
        Executar(async () =>
        {
            await mediator.Send(new DeleteMedicaoCommand(idColeta, idMedicao), cancellationToken);
            return NoContent();
        });

    private async Task<IActionResult> Executar(Func<Task<IActionResult>> acao)
    {
        try
        {
            return await acao();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
