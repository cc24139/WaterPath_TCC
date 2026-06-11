using MediatR;

namespace Application.Commands.Codigo;

public class CommandGerarCodigo : IRequest<string>
{
    public string UsuarioEmail { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandGerarCodigo() { }

    public CommandGerarCodigo(string usuarioEmail)
    {
        UsuarioEmail = usuarioEmail;
    }

}

public class CommandValidarCodigo : IRequest<string>
{
    public string UsuarioEmail { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public CommandValidarCodigo() { }

    public CommandValidarCodigo(string usuarioEmail, string codigo)
    {
        UsuarioEmail = usuarioEmail;
        Codigo = codigo;
    }
}

public class CommandAlterarSenhaCodigo() : IRequest<string>
{
    public string codigo { get; set; }
    public string email { get; set; }
    public string senha { get; set; }
}
