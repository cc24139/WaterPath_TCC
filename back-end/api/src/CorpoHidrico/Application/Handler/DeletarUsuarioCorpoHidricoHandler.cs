using Application.Commands.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using MediatR;

namespace back_end.src.Application.Handler.CorpoHidrico
{
    public class DeletarUsuarioCorpoHidricoHandler : IRequestHandler<CommandDeletarUsuario, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;

        public DeletarUsuarioCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
        }

        public async Task<Unit> Handle(CommandDeletarUsuario request, CancellationToken cancellationToken)
        {
            await corpoHidricoRepository.DeletarUsuario(request.CorpoHidricoId, request.UserId, cancellationToken);
            return Unit.Value;
        }
    }
}
