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
            corpoHidricoRepository.Atualizar(request.CorpoHidrico);
            return Task.FromResult(Unit.Value);
        }
    }
}
