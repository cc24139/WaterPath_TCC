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
                var existeCodigoPendente = await mediator.Send(new QueryCodigoPendente(result.email));
                if (existeCodigoPendente)
                {
                    return Unauthorized(new { mensagem = "Código de verificação pendente. Por favor, verifique seu email." });
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new{ mensagem = ex.Message });
            }
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] CommandCriarConta command)
        {
            try
            {
                var existingCode = await mediator.Send(new QueryCodigoPendente(command.Email));
                if (existingCode)                {
                    return Unauthorized(new { mensagem = "Já existe um código de verificação pendente para este email. Por favor, verifique seu email." });
                }
                var hash = new HashServices();
                command.Senha = hash.ComputeHash(command.Senha);
                var result = await mediator.Send(command);
                var emailService = new EmailServices();
                var codResult = await mediator.Send(new CommandGerarCodigo { UsuarioEmail = result.Email });
                await emailService.EnviarCodigoCadastro(command.Email, codResult.ToString());
                return Ok(new { id = result.UsuarioId, email = result.Email, mensagem = result.Mensagem });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
