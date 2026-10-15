using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using Domain.User;
using MediatR;

namespace back_end.src.Application.Handler.CorpoHidrico
{
    public class CadastrarCorpoHidricoHandler : IRequestHandler<CommandCadastrarCorpoHidrico, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;
        private readonly IUserRepository userRepository;

        public CadastrarCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository, IUserRepository userRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
            this.userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            CommandCadastrarCorpoHidrico request,
            CancellationToken cancellationToken
        )
        {
            var corpoHidrico = new CorpoHidricoEntity(
                request.Nome,
                request.Localizacao,
                request.Tamanho,
                request.EhPrivado
            );
            var usuarios = await userRepository.ObterUsuariosPorIds(request.UserIds, cancellationToken);
            corpoHidrico.users.AddRange(usuarios);
            await corpoHidricoRepository.Cadastrar(corpoHidrico, cancellationToken);
            return Unit.Value;
        }
    }
}
