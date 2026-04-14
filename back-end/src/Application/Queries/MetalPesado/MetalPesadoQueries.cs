using back_end.src.Domain.MetalPesado;
using MediatR;

namespace Application.Queries.MetalPesado;

public class QueryObterMetalPesadoPorId : IRequest<MetalPesadoEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodosMetaisPesados : IRequest<List<MetalPesadoEntity>> { }
