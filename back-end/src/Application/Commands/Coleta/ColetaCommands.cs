using back_end.src.Domain.Coleta;
using MediatR;

namespace Application.Commands.Coleta;

public class CommandCadastrarColeta : IRequest<Unit>
{
    public ColetaEntity Coleta { get; set; } = new();
}

public class CommandAtualizarColeta : IRequest<Unit>
{
    public ColetaEntity Coleta { get; set; } = new();
}

public class CommandDeletarColeta : IRequest<Unit>
{
    public int Id { get; set; }
}

