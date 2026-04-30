using back_end.src.Domain.QualidadeFutura;
using MediatR;

namespace Application.Queries.QualidadeFutura;

public class QueryObterQualidadeFuturaPorId : IRequest<QualidadeFuturaEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasQualidadesFuturas : IRequest<List<QualidadeFuturaEntity>> { }
