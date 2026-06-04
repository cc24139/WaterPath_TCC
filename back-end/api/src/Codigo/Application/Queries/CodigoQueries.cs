using MediatR;

namespace Application.Queries.Codigo;

public class QueryCodigoPendente : IRequest<bool>
{
    public string UsuarioEmail { get; set; }

    public QueryCodigoPendente() { }

    public QueryCodigoPendente(string usuarioEmail)
    {
        UsuarioEmail = usuarioEmail;
    }
}
