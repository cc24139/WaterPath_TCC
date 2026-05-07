using MediatR;

namespace Application.Queries.Codigo;

public class QueryCodigoPendente : IRequest<bool>
{
    public int UsuarioId { get; set; }

    public QueryCodigoPendente() { }

    public QueryCodigoPendente(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}
