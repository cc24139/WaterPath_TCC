using Application.Commands.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using MediatR;

namespace back_end.src.Application.Handler.CorpoHidrico
{
    public class DeletarCorpoHidricoHandler : IRequestHandler<CommandDeletarCorpoHidrico, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;

        public DeletarCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
        }

        public async Task<Unit> Handle(
            CommandDeletarCorpoHidrico request,
            CancellationToken cancellationToken
        )
        {
            await corpoHidricoRepository.Deletar(request.Id, cancellationToken);
            return Unit.Value;
        }
    }
}
