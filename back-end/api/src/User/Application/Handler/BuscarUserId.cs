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

    public async Task<UserEntity> Handle(QueryUserId query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.ObterUsuarioPorId(query.Id, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }
        return user;
    }
}
