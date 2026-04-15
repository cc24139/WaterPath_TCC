using Application.Commands.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Infrastructure.Repository;
using MediatR;

namespace back_end.src.Application.Handler.CorpoHidrico
{
    public class AdicionarUsuarioCorpoHidricoHandler
        : IRequestHandler<CommandAdicionarUsuario, Unit>
    {
        private readonly ICorpoHidricoRepository corpoHidricoRepository;

        public AdicionarUsuarioCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
        {
            this.corpoHidricoRepository = corpoHidricoRepository;
        }

        public Task<Unit> Handle(
            CommandAdicionarUsuario request,
            CancellationToken cancellationToken
        )
        {
            corpoHidricoRepository.AdicionarUsuario(request.CorpoHidricoId, request.UserId);
            return Task.FromResult(Unit.Value);
        }
    }
}
