using Domain.User;
using MediatR;

namespace Application.Queries.User;

public class QueryObterUsuarioPorId : IRequest<UserEntity?>
{
    public int Id { get; set; }
}

public class QueryObterUsuarioPorEmail : IRequest<UserEntity?>
{
    public string Email { get; set; } = string.Empty;
}

public class QueryObterTodosUsuarios : IRequest<List<UserEntity>> { }
