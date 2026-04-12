using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Application.Queries.User;
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

        public Task<UserLoginResponse> Handle(
            QueryLoginUser query,
            CancellationToken cancellationToken
        )
        {
            var user = userRepository.ObterUsuarioPorEmail(query.email);
            if (user == null || !new HashServices().VerifyHash(query.senha, user.Senha))
            {
                throw new ArgumentException("Email ou senha inválidos");
            }
            return Task.FromResult(new UserLoginResponse { email = user.Email, nome = user.Nome });
        }
    }
}
