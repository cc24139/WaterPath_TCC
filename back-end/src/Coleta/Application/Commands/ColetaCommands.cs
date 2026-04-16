using back_end.src.Domain.Coleta;
using MediatR;

namespace Application.Commands.Coleta;

public class CommandCadastrarColeta : IRequest<Unit>
{
    public ColetaEntity Coleta { get; set; } = new();
    public int CorpoHidricoId { get; set; }
}

public class CommandCadastrarListaColetas : IRequest<Unit>
{
    public List<ColetaEntity> Coletas { get; set; } = new();
}

public class CommandAtualizarColeta : IRequest<Unit>
{
    public ColetaEntity Coleta { get; set; } = new();
    public int ColetaId { get; set; }
}

public class CommandDeletarColeta : IRequest<Unit>
{
    public int Id { get; set; }
}
