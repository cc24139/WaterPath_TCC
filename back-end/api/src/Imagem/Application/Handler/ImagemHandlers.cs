using Application.Commands.Imagem;
using Application.Queries.Imagem;
using back_end.src.Domain.Imagem;
using MediatR;

namespace Application.Handler.Imagem;

public class CadastrarImagemHandler : IRequestHandler<CommandCadastrarImagem, Unit>
{
    private readonly IImagemRepository imagemRepository;

    public CadastrarImagemHandler(IImagemRepository imagemRepository)
    {
        this.imagemRepository = imagemRepository;
    }

    public async Task<Unit> Handle(CommandCadastrarImagem request, CancellationToken cancellationToken)
    {
        await imagemRepository.Cadastrar(request.Imagem, cancellationToken);
        return Unit.Value;
    }
}

public class AtualizarImagemHandler : IRequestHandler<CommandAtualizarImagem, Unit>
{
    private readonly IImagemRepository imagemRepository;

    public AtualizarImagemHandler(IImagemRepository imagemRepository)
    {
        this.imagemRepository = imagemRepository;
    }

    public async Task<Unit> Handle(CommandAtualizarImagem request, CancellationToken cancellationToken)
    {
        await imagemRepository.Atualizar(request.Imagem, cancellationToken);
        return Unit.Value;
    }
}

public class DeletarImagemHandler : IRequestHandler<CommandDeletarImagem, Unit>
{
    private readonly IImagemRepository imagemRepository;

    public DeletarImagemHandler(IImagemRepository imagemRepository)
    {
        this.imagemRepository = imagemRepository;
    }

    public async Task<Unit> Handle(CommandDeletarImagem request, CancellationToken cancellationToken)
    {
        await imagemRepository.Deletar(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class ObterImagemPorIdHandler : IRequestHandler<QueryObterImagemPorId, ImagemEntity?>
{
    private readonly IImagemRepository imagemRepository;

    public ObterImagemPorIdHandler(IImagemRepository imagemRepository)
    {
        this.imagemRepository = imagemRepository;
    }

    public Task<ImagemEntity?> Handle(
        QueryObterImagemPorId request,
        CancellationToken cancellationToken
    )
    {
        return imagemRepository.ObterPorId(request.Id, cancellationToken);
    }
}

public class ObterTodasImagensHandler : IRequestHandler<QueryObterTodasImagens, List<ImagemEntity>>
{
    private readonly IImagemRepository imagemRepository;

    public ObterTodasImagensHandler(IImagemRepository imagemRepository)
    {
        this.imagemRepository = imagemRepository;
    }

    public Task<List<ImagemEntity>> Handle(
        QueryObterTodasImagens request,
        CancellationToken cancellationToken
    )
    {
        return imagemRepository.ObterTodos(cancellationToken);
    }
}
