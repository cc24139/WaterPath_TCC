using back_end.src.Domain.Imagem;
using MediatR;

namespace Application.Commands.Imagem;

public class CommandCadastrarImagem : IRequest<Unit>
{
    public ImagemEntity Imagem { get; set; } = new();
}

public class CommandAtualizarImagem : IRequest<Unit>
{
    public ImagemEntity Imagem { get; set; } = new();
}

public class CommandDeletarImagem : IRequest<Unit>
{
    public int Id { get; set; }
}
