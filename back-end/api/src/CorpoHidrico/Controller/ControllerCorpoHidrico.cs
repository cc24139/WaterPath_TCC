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

namespace back_end.src.Controllers.CorpoHidrico
{
    [ApiController]
    [Route("api/corpohidrico")]
    public class ControllerCorpoHidrico : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerCorpoHidrico(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CommandCadastrarCorpoHidrico command)
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
                await mediator.Send(command);
                return Ok("Corpo hídrico cadastrado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorId { Id = id });
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

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            try
            {
                var corposHidricos = await mediator.Send(new QueryObterTodosCorposHidricos());
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
        public async Task<IActionResult> ObterPorUsuario()
        {
            try
            {
                var id = int.Parse(
                    User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                        ?? "0"
                );
                var corposHidricos = await mediator.Send(
                    new QueryObterCorposHidricosPorUsuario { UserId = id }
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
        public async Task<IActionResult> ObterPorUsuario(string nome)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new CorpoHidricoNomeCommand { Nome = nome });
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
            [FromBody] CommandAtualizarCorpoHidrico corpoHidrico
        )
        {
            try
            {
                corpoHidrico.Id = id;
                await mediator.Send(corpoHidrico);
                return Ok("Corpo hídrico atualizado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                await mediator.Send(new CommandDeletarCorpoHidrico { Id = id });
                return Ok("Corpo hídrico deletado com sucesso");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("nome/{nome}")]
        public async Task<IActionResult> ObterPorNome(string nome)
        {
            try
            {
                var corpoHidrico = await mediator.Send(new QueryObterCorpoHidricoPorNome { Nome = nome });
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
