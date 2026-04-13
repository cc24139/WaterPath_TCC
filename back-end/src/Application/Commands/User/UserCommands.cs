using MediatR;

namespace Application.Commands.User;

public class CommandCriarConta : IRequest<CriarContaResponse>
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }

    public CommandCriarConta()
    {
        Nome = string.Empty;
        Email = string.Empty;
        Senha = string.Empty;
    }

    public CommandCriarConta(string nome, string email, string senha)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
    }
}

public class CriarContaResponse
{
    public int UsuarioId { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
