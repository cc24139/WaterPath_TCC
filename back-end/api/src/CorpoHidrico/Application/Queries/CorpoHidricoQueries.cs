using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Queries.CorpoHidrico;

public class QueryObterCorpoHidricoPorId : IRequest<CorpoHidricoEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodosCorposHidricos : IRequest<List<CorpoHidricoEntity>> { }
public class QueryObterCorposHidricosPorUsuario : IRequest<List<CorpoHidricoEntity>>
{
    public int UserId { get; set; }
}

public class QueryObterUsuarioDOCorpoHidrico : IRequest<List<int>>
{
    public int CorpoHidricoId { get; set; }
}

public class QueryObterColetasDoCorpoHidrico : IRequest<List<int>>
{
    public int CorpoHidricoId { get; set; }
}   
