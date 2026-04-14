using back_end.src.Domain.Qualidade;
using MediatR;

namespace Application.Queries.Qualidade;

public class QueryObterQualidadePorId : IRequest<QualidadeEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasQualidades : IRequest<List<QualidadeEntity>> { }
