using System;
using System.Linq;
using System.Security.Claims;
using Application.Commands.CorpoHidrico;
using Application.Queries.CorpoHidrico;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Authorization;

namespace back_end.src.Controllers.CorpoHidrico
{
    [ApiController]
    [Route("api/corpohidrico")]
    public class ControllerCorpoHidrico : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ResourceAuthorizationService authorization;

        public ControllerCorpoHidrico(IMediator mediator, ResourceAuthorizationService authorization)
        {
            this.mediator = mediator;
            this.authorization = authorization;
        }

        private Task<bool> PodeAlterar(int id, CancellationToken cancellationToken)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
                return Task.FromResult(false);
            return authorization.PodeGerenciarCorpoHidricoAsync(id, userId, cancellationToken);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CommandCadastrarCorpoHidrico command, CancellationToken cancellationToken)
        {
            try
            {
                command.UserIds.Add(
                    int.Parse(
                        User.Claims.FirstOrDefault(static c =>
                            c.Type == ClaimTypes.NameIdentifier
                        )?.Value
                            ?? "0"
                    )
                );
                await mediator.Send(command, cancellationToken);
                return Ok("Corpo hídrico cadastrado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorId { Id = id }, cancellationToken);
                if (corpoHidrico == null)
                    return NotFound("Corpo hídrico não encontrado");
                //salva o corpoHidrico no token para consultas
                var corpoHidricoClaim = User.Claims.FirstOrDefault(c => c.Type == "corpoHidricoId");
                if (corpoHidricoClaim != null)
                    corpoHidricoClaim = new Claim("corpoHidricoId", corpoHidrico.Id.ToString());
                else
                    ((ClaimsIdentity)User.Identity).AddClaim(
                        new Claim("corpoHidricoId", corpoHidrico.Id.ToString())
                    );
                return Ok(CorpoHidricoDTO.FromEntity(corpoHidrico));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}/risco-atual")]
        public async Task<IActionResult> ObterRiscoAtual(int id, CancellationToken cancellationToken)
        {
            try
            {
                var risco = await mediator.Send(new QueryObterRiscoAtual(id), cancellationToken);
                return risco is null
                    ? NotFound("Corpo hídrico sem resultado de risco válido")
                    : Ok(risco);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
        {
            try
            {
                var corposHidricos = await mediator.Send(new QueryObterTodosCorposHidricos(), cancellationToken);
                var dtos = CorpoHidricoDTO.FromEntities(corposHidricos);
                return Ok(dtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("usuario")]
        public async Task<IActionResult> ObterPorUsuario(CancellationToken cancellationToken)
        {
            try
            {
                var id = int.Parse(
                    User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                        ?? "0"
                );
                var corposHidricos = await mediator.Send(
                    new QueryObterCorposHidricosPorUsuario { UserId = id },
                    cancellationToken
                );
                if (corposHidricos == null || !corposHidricos.Any())
                    return NotFound("Corpos hídricos não encontrados para o usuário");
                return Ok(CorpoHidricoDTO.FromEntities(corposHidricos));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("usuario/{nome}")]
        public async Task<IActionResult> ObterPorUsuario(string nome, CancellationToken cancellationToken)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new CorpoHidricoNomeCommand { Nome = nome }, cancellationToken);
                if (corpoHidrico == null)
                    return NotFound("Corpo hídrico não encontrado");
                return Ok(CorpoHidricoDTO.FromEntity(corpoHidrico));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] CommandAtualizarCorpoHidrico corpoHidrico,
            CancellationToken cancellationToken
        )
        {
            try
            {
                if (!await PodeAlterar(id, cancellationToken)) return Forbid();
                corpoHidrico.Id = id;
                await mediator.Send(corpoHidrico, cancellationToken);
                return Ok("Corpo hídrico atualizado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id, CancellationToken cancellationToken)
        {
            try
            {
                if (!await PodeAlterar(id, cancellationToken)) return Forbid();
                await mediator.Send(new CommandDeletarCorpoHidrico { Id = id }, cancellationToken);
                return Ok("Corpo hídrico deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("nome/{nome}")]
        public async Task<IActionResult> ObterPorNome(string nome, CancellationToken cancellationToken)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorNome { Nome = nome }, cancellationToken);
                if (corpoHidrico == null)
                    return NotFound("Corpo hídrico não encontrado");
                return Ok(CorpoHidricoDTO.FromEntity(corpoHidrico));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}
