using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.User;
using Application.Handler.HandlerUser;
using back_end.src.Application.Handler.User;
using back_end.src.Application.Queries.User;
using back_end.src.Infrastructure.Services;
using back_end.src.Infrastructure.Services.Interfaces;
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
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
