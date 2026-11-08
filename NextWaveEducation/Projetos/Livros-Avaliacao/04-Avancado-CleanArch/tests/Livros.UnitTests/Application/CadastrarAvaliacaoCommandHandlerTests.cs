using FluentAssertions;
using Livros.Application.Avaliacoes.Commands.CadastrarAvaliacao;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Livros.UnitTests.Application;

public class CadastrarAvaliacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILivroRepository> _livroRepository = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly CadastrarAvaliacaoCommandHandler _handler;

    public CadastrarAvaliacaoCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.Livros).Returns(_livroRepository.Object);
        _unitOfWork.Setup(u => u.Usuarios).Returns(_usuarioRepository.Object);

        _handler = new CadastrarAvaliacaoCommandHandler(
            _unitOfWork.Object,
            NullLogger<CadastrarAvaliacaoCommandHandler>.Instance);
    }

    private static Livro CriarLivro() => new(
        "Domain-Driven Design",
        "Tackling complexity in the heart of software.",
        "978-0321125217",
        "Eric Evans",
        "Addison-Wesley",
        GeneroLivroEnum.Tecnico,
        2003,
        560);

    private static Usuario CriarUsuario() => new("Ana Souza", "ana.souza@example.com");

    private void Configurar(Livro livro, Usuario usuario)
    {
        _livroRepository
            .Setup(r => r.ObterPorIdComAvaliacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(livro);

        _usuarioRepository
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
    }

    private static CadastrarAvaliacaoCommand ComandoValido(int nota = 5) => new()
    {
        IdLivro = 1,
        IdUsuario = 1,
        Nota = nota,
        Descricao = "Excelente livro.",
        DataInicioLeitura = DateTime.Today.AddDays(-30),
        DataFimLeitura = DateTime.Today.AddDays(-10)
    };

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCadastrarEComitar()
    {
        Configurar(CriarLivro(), CriarUsuario());

        var resultado = await _handler.Handle(ComandoValido(), CancellationToken.None);

        resultado.Avaliacao.Nota.Should().Be(5);
        resultado.Avaliacao.NomeUsuario.Should().Be("Ana Souza");
        resultado.TituloLivro.Should().Be("Domain-Driven Design");
        resultado.TotalAvaliacoesDoLivro.Should().Be(1);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComLivroInexistente_DeveLancarNaoEncontrado()
    {
        _livroRepository
            .Setup(r => r.ObterPorIdComAvaliacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Livro?)null);

        var acao = async () => await _handler.Handle(ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<NaoEncontradoException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComUsuarioInexistente_DeveLancarNaoEncontrado()
    {
        _livroRepository
            .Setup(r => r.ObterPorIdComAvaliacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarLivro());

        _usuarioRepository
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var acao = async () => await _handler.Handle(ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<NaoEncontradoException>();
    }

    // ----- Regra: a nota deve ser de 1 a 5 -----

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Handle_ComNotaForaDaFaixa_DeveLancarRegraNegocio(int nota)
    {
        Configurar(CriarLivro(), CriarUsuario());

        var acao = async () => await _handler.Handle(ComandoValido(nota), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*nota deve ser de 1 a 5*");

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ----- Regra: data de início não pode ser maior que a de fim -----

    [Fact]
    public async Task Handle_ComInicioMaiorQueOFim_DeveLancarRegraNegocio()
    {
        Configurar(CriarLivro(), CriarUsuario());

        var comando = ComandoValido() with
        {
            DataInicioLeitura = DateTime.Today.AddDays(-5),
            DataFimLeitura = DateTime.Today.AddDays(-20)
        };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*não pode ser maior que a data de fim*");
    }

    [Fact]
    public async Task Handle_ComFimNoFuturo_DeveLancarRegraNegocio()
    {
        Configurar(CriarLivro(), CriarUsuario());

        var comando = ComandoValido() with
        {
            DataInicioLeitura = DateTime.Today,
            DataFimLeitura = DateTime.Today.AddDays(5)
        };

        var acao = async () => await _handler.Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    [Fact]
    public async Task Handle_QuandoOUsuarioJaAvaliouOLivro_DeveLancarRegraNegocio()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;
        livro.AdicionarAvaliacao(1, 4, "Primeira", hoje.AddDays(-60), hoje.AddDays(-50));

        Configurar(livro, CriarUsuario());

        var acao = async () => await _handler.Handle(ComandoValido(), CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*já avaliou*");
    }

    [Fact]
    public async Task Handle_DeveCalcularOsDiasDeLeitura()
    {
        Configurar(CriarLivro(), CriarUsuario());

        var comando = ComandoValido() with
        {
            DataInicioLeitura = DateTime.Today.AddDays(-9),
            DataFimLeitura = DateTime.Today
        };

        var resultado = await _handler.Handle(comando, CancellationToken.None);

        resultado.Avaliacao.DiasDeLeitura.Should().Be(10);
    }
}
