namespace Application.Handler.HandlerUser;

using Application.Queries.User;
using Domain.User;

public class HandlerBuscarUserId
{
    private readonly IUserRepository userRepository;

    public HandlerBuscarUserId(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public UserEntity Handle(QueryUserId query)
    {
        var user = userRepository.ObterUsuarioPorId(query.Id);
        if (user == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }
        return user;
    }
}
