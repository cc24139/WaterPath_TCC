using back_end.src.Domain.CianoBacteria;
using MediatR;

namespace Application.Commands.CianoBacteria;

public class CommandCadastrarCianoBacteria : IRequest<Unit>
{
    public CianoBacteriaEntity CianoBacteria { get; set; } = new();
}

public class CommandAtualizarCianoBacteria : IRequest<Unit>
{
    public CianoBacteriaEntity CianoBacteria { get; set; } = new();
}

public class CommandDeletarCianoBacteria : IRequest<Unit>
{
    public int Id { get; set; }
}
