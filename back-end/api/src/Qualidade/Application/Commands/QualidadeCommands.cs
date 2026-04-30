using back_end.src.Domain.Qualidade;
using MediatR;

namespace Application.Commands.Qualidade;

public class CommandCadastrarQualidade : IRequest<Unit>
{
    public QualidadeEntity Qualidade { get; set; } = new();
}

public class CommandAtualizarQualidade : IRequest<Unit>
{
    public QualidadeEntity Qualidade { get; set; } = new();
}

public class CommandDeletarQualidade : IRequest<Unit>
{
    public int Id { get; set; }
}
