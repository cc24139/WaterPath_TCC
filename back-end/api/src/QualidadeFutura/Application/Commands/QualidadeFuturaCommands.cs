using back_end.src.Domain.QualidadeFutura;
using MediatR;

namespace Application.Commands.QualidadeFutura;

public class CommandCadastrarQualidadeFutura : IRequest<Unit>
{
    public QualidadeFuturaEntity QualidadeFutura { get; set; } = new();
}

public class CommandAtualizarQualidadeFutura : IRequest<Unit>
{
    public QualidadeFuturaEntity QualidadeFutura { get; set; } = new();
}

public class CommandDeletarQualidadeFutura : IRequest<Unit>
{
    public int Id { get; set; }
}
