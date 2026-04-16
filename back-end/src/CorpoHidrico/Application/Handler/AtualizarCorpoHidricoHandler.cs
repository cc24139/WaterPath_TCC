using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using MediatR;

namespace back_end.src.Application.Handler.CorpoHidrico
{
    public class AtualizarCorpoHidricoHandler : IRequestHandler<CommandAtualizarCorpoHidrico, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;

        public AtualizarCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
        }

        public Task<Unit> Handle(
            CommandAtualizarCorpoHidrico request,
            CancellationToken cancellationToken
        )
        {
            var corpoHidrico = new CorpoHidricoEntity(
                request.Nome,
                request.Localizacao,
                request.Tamanho,
                request.EhPrivado
            );

            corpoHidricoRepository.Atualizar(corpoHidrico);
            return Task.FromResult(Unit.Value);
        }
    }
}
