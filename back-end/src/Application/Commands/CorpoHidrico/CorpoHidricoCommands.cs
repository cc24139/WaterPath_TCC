using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Commands.CorpoHidrico;

public class CommandCadastrarCorpoHidrico : IRequest<Unit>
{
    public CorpoHidricoEntity CorpoHidrico { get; set; } = new();
}

public class CommandAtualizarCorpoHidrico : IRequest<Unit>
{
    public CorpoHidricoEntity CorpoHidrico { get; set; } = new();
}

public class CommandDeletarCorpoHidrico : IRequest<Unit>
{
    public int Id { get; set; }
}
