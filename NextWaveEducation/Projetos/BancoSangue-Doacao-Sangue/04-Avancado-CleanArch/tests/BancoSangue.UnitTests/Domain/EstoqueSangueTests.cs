using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Events;
using BancoSangue.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BancoSangue.UnitTests.Domain;

public class EstoqueSangueTests
{
    [Fact]
    public void Criar_DeveIniciarComAQuantidadeInformada()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.O, FatorRh.Negativo, 1000);

        estoque.QuantidadeML.Should().Be(1000);
        estoque.Descricao.Should().Be("O-");
    }

    [Theory]
    [InlineData(TipoSanguineo.A, FatorRh.Positivo, "A+")]
    [InlineData(TipoSanguineo.AB, FatorRh.Negativo, "AB-")]
    [InlineData(TipoSanguineo.O, FatorRh.Positivo, "O+")]
    [InlineData(TipoSanguineo.B, FatorRh.Negativo, "B-")]
    public void Descricao_DeveCombinarTipoEFatorRh(
        TipoSanguineo tipo, FatorRh fator, string esperado)
        => new EstoqueSangue(tipo, fator).Descricao.Should().Be(esperado);

    [Fact]
    public void Adicionar_DeveSomarNaQuantidade()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 1000);

        estoque.Adicionar(450);

        estoque.QuantidadeML.Should().Be(1450);
    }

    [Fact]
    public void Adicionar_ComQuantidadeInvalida_DeveLancarExcecao()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo);

        var acao = () => estoque.Adicionar(0);

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Remover_ComEstoqueSuficiente_DeveSubtrair()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 3000);

        estoque.Remover(500);

        estoque.QuantidadeML.Should().Be(2500);
    }

    [Fact]
    public void Remover_AcimaDoDisponivel_DeveLancarExcecao()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 400);

        var acao = () => estoque.Remover(500);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*Estoque insuficiente*");
    }

    // ----- Regra PLUS: avisar quando o estoque atingir a quantidade mínima -----

    [Fact]
    public void EstaAbaixoDoMinimo_ComEstoqueBaixo_DeveSerTrue()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 1999);

        estoque.EstaAbaixoDoMinimo.Should().BeTrue();
    }

    [Fact]
    public void EstaAbaixoDoMinimo_NoMinimoExato_DeveSerFalse()
    {
        var estoque = new EstoqueSangue(
            TipoSanguineo.A, FatorRh.Positivo, EstoqueSangue.QuantidadeMinimaML);

        estoque.EstaAbaixoDoMinimo.Should().BeFalse();
    }

    [Fact]
    public void Adicionar_SeContinuarAbaixoDoMinimo_DeveDispararEventoDeAlerta()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 100);

        estoque.Adicionar(450);

        estoque.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EstoqueAbaixoDoMinimoEvent>();
    }

    [Fact]
    public void Adicionar_SeUltrapassarOMinimo_NaoDeveDispararAlerta()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 1800);

        estoque.Adicionar(450);

        estoque.QuantidadeML.Should().Be(2250);
        estoque.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Remover_SeCairAbaixoDoMinimo_DeveDispararAlerta()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.O, FatorRh.Negativo, 2100);

        estoque.Remover(200);

        estoque.QuantidadeML.Should().Be(1900);
        estoque.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EstoqueAbaixoDoMinimoEvent>();
    }

    [Fact]
    public void EventoDeAlerta_DeveTrazerMensagemDescritiva()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.B, FatorRh.Negativo, 500);

        estoque.VerificarEstoqueMinimo();

        var evento = estoque.DomainEvents.OfType<EstoqueAbaixoDoMinimoEvent>().Single();
        evento.TipoSanguineo.Should().Be("B-");
        evento.QuantidadeAtualML.Should().Be(500);
        evento.QuantidadeMinimaML.Should().Be(EstoqueSangue.QuantidadeMinimaML);
        evento.Mensagem.Should().Contain("B-").And.Contain("500ml");
    }

    [Fact]
    public void LimparEventos_DeveEsvaziarAColecao()
    {
        var estoque = new EstoqueSangue(TipoSanguineo.A, FatorRh.Positivo, 100);
        estoque.VerificarEstoqueMinimo();

        estoque.LimparEventos();

        estoque.DomainEvents.Should().BeEmpty();
    }
}
