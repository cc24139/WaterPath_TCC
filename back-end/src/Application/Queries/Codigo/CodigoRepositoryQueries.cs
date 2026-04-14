using MediatR;

namespace Application.Queries.Codigo;

public class QueryVerificarPendenciaCodigo : IRequest<bool>
{
    public int UsuarioId { get; set; }
}
