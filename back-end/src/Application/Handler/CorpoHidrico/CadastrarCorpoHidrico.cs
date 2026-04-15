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
    public class CadastrarCorpoHidricoHandler : IRequestHandler<CommandCadastrarCorpoHidrico, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;

        public CadastrarCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
        }

        public Task<Unit> Handle(CommandCadastrarCorpoHidrico request, CancellationToken cancellationToken)
        {
            if(request.CorpoHidrico == null)
                throw new ArgumentNullException(nameof(request.CorpoHidrico));

            corpoHidricoRepository.Cadastrar(request.CorpoHidrico);
            return Task.FromResult(Unit.Value);
        }
    }
}