using System.Collections.Generic;

namespace Domain.User
{
    public interface IUserRepository
    {
        Task Cadastrar(UserEntity user, CancellationToken cancellationToken = default);
        Task Atualizar(UserEntity user, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<UserEntity?> ObterUsuarioPorId(int id, CancellationToken cancellationToken = default);
        Task<UserEntity?> ObterUsuarioPorEmail(string email, CancellationToken cancellationToken = default);
        Task<List<UserEntity>> ObterTodos(CancellationToken cancellationToken = default);
        Task<List<UserEntity>> ObterUsuariosPorIds(List<int> ids, CancellationToken cancellationToken = default);
    }
}
