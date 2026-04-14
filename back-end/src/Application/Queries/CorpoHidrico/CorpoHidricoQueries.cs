using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Queries.CorpoHidrico;

public class QueryObterCorpoHidricoPorId : IRequest<CorpoHidricoEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodosCorposHidricos : IRequest<List<CorpoHidricoEntity>> { }
