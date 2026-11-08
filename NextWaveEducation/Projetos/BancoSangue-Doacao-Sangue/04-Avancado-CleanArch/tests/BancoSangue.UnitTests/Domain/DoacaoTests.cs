using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BancoSangue.UnitTests.Domain;

public class DoacaoTests
{
    private static Doador CriarDoadorApto() => new(
        "Doador Apto",
        "apto@example.com",
        DateTime.Today.AddYears(-30),
        Genero.Masculino,
        80,
        TipoSanguineo.A,
        FatorRh.Positivo);

    // ----- Regra: quantidade entre 420ml e 470ml -----

    [Theory]
    [InlineData(420)]
    [InlineData(450)]
    [InlineData(470)]
    public void RegistrarDoacao_ComQuantidadeNaFaixaPermitida_DeveFuncionar(int quantidade)
    {
        var doador = CriarDoadorApto();

        var doacao = doador.RegistrarDoacao(quantidade, DateTime.Now);

        doacao.QuantidadeML.Should().Be(quantidade);
    }

    [Theory]
    [InlineData(419)]
    [InlineData(400)]
    [InlineData(0)]
    [InlineData(471)]
    [InlineData(500)]
    public void RegistrarDoacao_ComQuantidadeForaDaFaixa_DeveLancarExcecao(int quantidade)
    {
        var doador = CriarDoadorApto();

        var acao = () => doador.RegistrarDoacao(quantidade, DateTime.Now);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*entre 420ml e 470ml*");
    }

    [Fact]
    public void RegistrarDoacao_ComDataFutura_DeveLancarExcecao()
    {
        var doador = CriarDoadorApto();

        var acao = () => doador.RegistrarDoacao(450, DateTime.Now.AddDays(1));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    [Fact]
    public void RegistrarDoacao_DeveGuardarDataEDoadorId()
    {
        var doador = CriarDoadorApto();
        var data = DateTime.Now.AddDays(-5);

        var doacao = doador.RegistrarDoacao(440, data);

        doacao.DataDoacao.Should().BeCloseTo(data, TimeSpan.FromSeconds(1));
        doacao.DoadorId.Should().Be(doador.Id);
    }

    [Fact]
    public void LimitesDeQuantidade_DevemSeguirAEspecificacao()
    {
        Doacao.QuantidadeMinimaML.Should().Be(420);
        Doacao.QuantidadeMaximaML.Should().Be(470);
    }
}
