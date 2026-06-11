using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Application.Queries.User;
using back_end.src.Infrastructure.Repository;
using back_end.src.Infrastructure.Services;
using Domain.User;
using MediatR;

namespace back_end.src.Application.Handler.User
{
    public class LoginUserHandler
        : IRequestHandler<Queries.User.QueryLoginUser, Queries.User.UserLoginResponse>
    {
        public readonly IUserRepository userRepository;

        public LoginUserHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<UserLoginResponse> Handle(
            QueryLoginUser query,
            CancellationToken cancellationToken
        )
        {
            var user = await userRepository.ObterUsuarioPorEmail(query.email);
            if (user == null || !new HashServices().VerifyHash(query.senha, user.Senha))
            {
                throw new ArgumentException("Email ou senha inválidos");
            }
            var token = new JWTService().GenerateToken(user.Nome, user.Id, user.Email);
            var response = new UserLoginResponse { Id = user.Id, email = user.Email, nome = user.Nome, token = token };
            return await Task.FromResult(response);
        }
    }
}
