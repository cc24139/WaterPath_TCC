using System;
using Application.Commands.Codigo;
using Application.Commands.User;
using Application.Handler.Codigo;
using Application.Queries.Codigo;
using back_end.src.Application.Queries.User;
using back_end.src.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace back_end.src.Controllers.User
{
    [ApiController]
    [Route("/api/user")]
    public class ControllerUser : ControllerBase
    {
        private readonly IMediator mediator;

        public ControllerUser(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] QueryLoginUser query)
        {
            try
            {
                var result = await mediator.Send(query);
                var existeCodigoPendente = await mediator.Send(new QueryCodigoPendente(result.Id));
                if (existeCodigoPendente)
                {
                    return BadRequest(new { id = result.Id, email = result.email, nome = result.nome, pendente = true });
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] CommandCriarConta command)
        {
            try
            {
                var hash = new HashServices();
                command.Senha = hash.ComputeHash(command.Senha);
                var result = await mediator.Send(command);
                var emailService = new EmailServices();
                await mediator.Send(new CommandGerarCodigo { UsuarioId = result.UsuarioId });
                await emailService.EnviarCodigoCadastro(command.Email, "123456");
                return Ok(new { id = result.UsuarioId, mensagem = result.Mensagem });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
