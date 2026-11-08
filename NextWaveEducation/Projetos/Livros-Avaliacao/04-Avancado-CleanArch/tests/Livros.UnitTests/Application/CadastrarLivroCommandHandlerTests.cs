using FluentAssertions;
using Livros.Application.Livros.Commands.CadastrarLivro;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Livros.UnitTests.Application;

public class CadastrarLivroCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILivroRepository> _livroRepository = new();
    private readonly CadastrarLivroCommandHandler _handler;

    public CadastrarLivroCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.Livros).Returns(_livroRepository.Object);

        _handler = new CadastrarLivroCommandHandler(
            _unitOfWork.Object,
            NullLogger<CadastrarLivroCommandHandler>.Instance);
    }

    private static CadastrarLivroCommand ComandoValido() => new()
    {
        Titulo = "Domain-Driven Design",
        Descricao = "Tackling complexity in the heart of software.",
        Isbn = "978-0321125217",
        Autor = "Eric Evans",
        Editora = "Addison-Wesley",
        Genero = GeneroLivroEnum.Tecnico,
        AnoDePublicacao = 2003,
        QuantidadePaginas = 560
    };

    private void ConfigurarIsbnExistente(bool existe)
        => _livroRepository
            .Setup(r => r.ExisteIsbnAsync(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existe);

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCadastrarEComitar()
    {
        ConfigurarIsbnExistente(false);

        var resultado = await _handler.Handle(ComandoValido(), CancellationToken.None);

        resultado.Titulo.Should().Be("Domain-Driven Design");
        resultado.Autor.Should().Be("Eric Evans");
        resultado.Isbn.Should().Be("978-0321125217");
        resultado.Genero.Should().Be("Tecnico");
        resultado.NotaMedia.Should().Be(0);
        resultado.TotalAvaliacoes.Should().Be(0);
        resultado.TemCapa.Should().BeFalse();

        _livroRepository.Verify(
            r => r.AdicionarAsync(It.IsAny<Livro>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Regra: livros não podem ser cadastrados com o mesmo ISBN -----

    [Fact]
    public async Task Handle_ComIsbnJaCadastrado_DeveLancarExcecao()
    {
        ConfigurarIsbnExistente(true);

        var acao = async () => await _handler.Handle(ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Já existe um livro cadastrado com o ISBN*");

        _livroRepository.Verify(
            r => r.AdicionarAsync(It.IsAny<Livro>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveVerificarDuplicidadePeloIsbnNormalizado()
    {
        ConfigurarIsbnExistente(false);

        await _handler.Handle(ComandoValido(), CancellationToken.None);

        // O comando informou "978-0321125217"; a checagem usa a versão sem hífens.
        _livroRepository.Verify(
            r => r.ExisteIsbnAsync("9780321125217", It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComIsbnInvalido_DeveLancarExcecao()
    {
        ConfigurarIsbnExistente(false);

        var comando = ComandoValido() with { Isbn = "123" };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*10 ou 13*");
    }

    [Fact]
    public async Task Handle_ComTituloVazio_DeveLancarExcecao()
    {
        ConfigurarIsbnExistente(false);

        var comando = ComandoValido() with { Titulo = "" };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*título*");
    }

    [Fact]
    public async Task Handle_ComPaginasZero_DeveLancarExcecao()
    {
        ConfigurarIsbnExistente(false);

        var comando = ComandoValido() with { QuantidadePaginas = 0 };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*páginas*");
    }
}
