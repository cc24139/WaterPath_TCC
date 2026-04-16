using Domain.User;
using MediatR;

namespace Application.Commands.User;

public class CommandCadastrarUsuario : IRequest<Unit>
{
    public UserEntity Usuario { get; set; } = new();
}

public class CommandAtualizarUsuario : IRequest<Unit>
{
    public UserEntity Usuario { get; set; } = new();
}

public class CommandDeletarUsuario : IRequest<Unit>
{
    public int Id { get; set; }
}
