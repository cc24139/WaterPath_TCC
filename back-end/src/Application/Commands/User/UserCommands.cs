using MediatR;

namespace Application.Commands.User;

public class CommandCriarConta : IRequest<string>
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }

    public CommandCriarConta(string nome, string email, string senha)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
    }
}