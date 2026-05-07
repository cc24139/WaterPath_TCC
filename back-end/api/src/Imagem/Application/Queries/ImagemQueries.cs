using back_end.src.Domain.Imagem;
using MediatR;

namespace Application.Queries.Imagem;

public class QueryObterImagemPorId : IRequest<ImagemEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasImagens : IRequest<List<ImagemEntity>> { }
