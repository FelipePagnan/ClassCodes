using BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;
using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BancoSangue.UnitTests.Application;

public class RegistrarDoacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDoadorRepository> _doadorRepository = new();
    private readonly Mock<IDoacaoRepository> _doacaoRepository = new();
    private readonly Mock<IEstoqueSangueRepository> _estoqueRepository = new();
    private readonly RegistrarDoacaoCommandHandler _handler;

    public RegistrarDoacaoCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.Doadores).Returns(_doadorRepository.Object);
        _unitOfWork.Setup(u => u.Doacoes).Returns(_doacaoRepository.Object);
        _unitOfWork.Setup(u => u.EstoquesSangue).Returns(_estoqueRepository.Object);

        _handler = new RegistrarDoacaoCommandHandler(
            _unitOfWork.Object,
            NullLogger<RegistrarDoacaoCommandHandler>.Instance);
    }

    private static Doador CriarDoador(
        Genero genero = Genero.Masculino,
        double peso = 80,
        int idade = 30)
        => new(
            "Doador Teste",
            "doador@example.com",
            DateTime.Today.AddYears(-idade),
            genero,
            peso,
            TipoSanguineo.O,
            FatorRh.Negativo);

    private void ConfigurarDoador(Doador doador)
        => _doadorRepository
            .Setup(r => r.ObterPorIdComDoacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(doador);

    private void ConfigurarEstoque(int quantidadeML)
        => _estoqueRepository
            .Setup(r => r.ObterPorTipoAsync(
                It.IsAny<TipoSanguineo>(), It.IsAny<FatorRh>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EstoqueSangue(TipoSanguineo.O, FatorRh.Negativo, quantidadeML));

    [Fact]
    public async Task Handle_ComDoacaoValida_DeveRegistrarEComitar()
    {
        ConfigurarDoador(CriarDoador());
        ConfigurarEstoque(5000);

        var resultado = await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        resultado.Doacao.QuantidadeML.Should().Be(450);
        resultado.NomeDoador.Should().Be("Doador Teste");
        resultado.AlertaEstoque.Should().BeNull();

        _doacaoRepository.Verify(
            r => r.AdicionarAsync(It.IsAny<Doacao>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComDoadorInexistente_DeveLancarNaoEncontrado()
    {
        _doadorRepository
            .Setup(r => r.ObterPorIdComDoacoesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doador?)null);

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 999, QuantidadeML = 450 },
            CancellationToken.None);

        await acao.Should().ThrowAsync<NaoEncontradoException>();
    }

    [Fact]
    public async Task Handle_ComDoadorMenorDeIdade_DeveLancarRegraNegocio()
    {
        ConfigurarDoador(CriarDoador(idade: 16));

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*Menores de idade não podem doar*");

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComDoadorAbaixoDoPesoMinimo_DeveLancarRegraNegocio()
    {
        ConfigurarDoador(CriarDoador(peso: 48));

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*peso mínimo*");
    }

    [Theory]
    [InlineData(419)]
    [InlineData(471)]
    public async Task Handle_ComQuantidadeForaDaFaixa_DeveLancarRegraNegocio(int quantidade)
    {
        ConfigurarDoador(CriarDoador());

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = quantidade },
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*entre 420ml e 470ml*");
    }

    [Fact]
    public async Task Handle_HomemAntesDeSessentaDias_DeveLancarRegraNegocio()
    {
        var doador = CriarDoador(Genero.Masculino);
        doador.RegistrarDoacao(450, DateTime.Now.AddDays(-30));
        ConfigurarDoador(doador);

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*intervalo mínimo de 60 dias*");
    }

    [Fact]
    public async Task Handle_MulherAntesDeNoventaDias_DeveLancarRegraNegocio()
    {
        var doadora = CriarDoador(Genero.Feminino);
        doadora.RegistrarDoacao(450, DateTime.Now.AddDays(-70));
        ConfigurarDoador(doadora);

        var acao = async () => await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        await acao.Should().ThrowAsync<RegraNegocioException>()
            .WithMessage("*intervalo mínimo de 90 dias*");
    }

    // ----- Regra PLUS: avisar quando o estoque fica abaixo do mínimo -----

    [Fact]
    public async Task Handle_QuandoOEstoqueFicaBaixo_DeveRetornarAlerta()
    {
        ConfigurarDoador(CriarDoador());
        ConfigurarEstoque(800);

        var resultado = await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        resultado.AlertaEstoque.Should().NotBeNull();
        resultado.EstoqueAtualizado.EstoqueBaixo.Should().BeTrue();
        resultado.AlertaEstoque.Should().Contain("O-");
    }

    [Fact]
    public async Task Handle_QuandoOEstoqueEstaSaudavel_NaoDeveRetornarAlerta()
    {
        ConfigurarDoador(CriarDoador());
        ConfigurarEstoque(10000);

        var resultado = await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        resultado.AlertaEstoque.Should().BeNull();
        resultado.EstoqueAtualizado.EstoqueBaixo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SemDataInformada_DeveUsarADataAtual()
    {
        ConfigurarDoador(CriarDoador());
        ConfigurarEstoque(5000);

        var resultado = await _handler.Handle(
            new RegistrarDoacaoCommand { DoadorId = 1, QuantidadeML = 450 },
            CancellationToken.None);

        resultado.Doacao.DataDoacao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }
}
