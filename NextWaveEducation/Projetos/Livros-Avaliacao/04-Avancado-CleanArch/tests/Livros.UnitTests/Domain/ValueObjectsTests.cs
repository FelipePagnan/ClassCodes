using FluentAssertions;
using Livros.Domain.Exceptions;
using Livros.Domain.ValueObjects;
using Xunit;

namespace Livros.UnitTests.Domain;

public class IsbnTests
{
    [Theory]
    [InlineData("978-0321125217", "9780321125217")]
    [InlineData("9780321125217", "9780321125217")]
    [InlineData("978 0321 125217", "9780321125217")]
    [InlineData("0-321-12521-7", "0321125217")]
    public void Criar_ComIsbnValido_DeveNormalizar(string entrada, string esperado)
    {
        var isbn = Isbn.Criar(entrada);

        isbn.Normalizado.Should().Be(esperado);
        isbn.Valor.Should().Be(entrada.Trim());
    }

    [Fact]
    public void Criar_DoisFormatosDoMesmoIsbn_DevemNormalizarIgual()
    {
        var comHifens = Isbn.Criar("978-0321125217");
        var semHifens = Isbn.Criar("9780321125217");

        comHifens.Normalizado.Should().Be(semHifens.Normalizado);
    }

    [Fact]
    public void Criar_ComIsbn10TerminandoEmX_DevePermitir()
    {
        var isbn = Isbn.Criar("043942089X");

        isbn.Normalizado.Should().Be("043942089X");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComValorVazio_DeveLancarExcecao(string? valor)
    {
        var acao = () => Isbn.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*obrigatório*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678")]
    [InlineData("12345678901")]
    [InlineData("12345678901234")]
    public void Criar_ComQuantidadeDeDigitosInvalida_DeveLancarExcecao(string valor)
    {
        var acao = () => Isbn.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*10 ou 13*");
    }

    [Fact]
    public void Criar_ComIsbn13ContendoX_DeveLancarExcecao()
    {
        var acao = () => Isbn.Criar("978032112521X");

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*ISBN-13*");
    }

    [Fact]
    public void Criar_ComXNoMeioDeUmIsbn10_DeveLancarExcecao()
    {
        var acao = () => Isbn.Criar("03X1125217");

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*último dígito*");
    }

    [Theory]
    [InlineData("978-0321125217", true)]
    [InlineData("0321125217", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void EhValido_DeveRefletirAValidacao(string valor, bool esperado)
        => Isbn.EhValido(valor).Should().Be(esperado);
}

public class PeriodoLeituraTests
{
    // ----- Regra: data de início não pode ser maior que a de fim -----

    [Fact]
    public void Criar_ComInicioAntesDoFim_DevePermitir()
    {
        var hoje = DateTime.Today;

        var periodo = PeriodoLeitura.Criar(hoje.AddDays(-30), hoje.AddDays(-10));

        periodo.Inicio.Should().Be(hoje.AddDays(-30));
        periodo.Fim.Should().Be(hoje.AddDays(-10));
    }

    [Fact]
    public void Criar_ComInicioIgualAoFim_DevePermitir()
    {
        var hoje = DateTime.Today;

        var acao = () => PeriodoLeitura.Criar(hoje, hoje);

        acao.Should().NotThrow();
    }

    [Fact]
    public void Criar_ComInicioMaiorQueOFim_DeveLancarExcecao()
    {
        var hoje = DateTime.Today;

        var acao = () => PeriodoLeitura.Criar(hoje.AddDays(-5), hoje.AddDays(-20));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não pode ser maior que a data de fim*");
    }

    [Fact]
    public void Criar_ComInicioUmDiaDepoisDoFim_DeveLancarExcecao()
    {
        var hoje = DateTime.Today;

        var acao = () => PeriodoLeitura.Criar(hoje.AddDays(-9), hoje.AddDays(-10));

        acao.Should().Throw<RegraNegocioException>();
    }

    [Fact]
    public void Criar_ComFimNoFuturo_DeveLancarExcecao()
    {
        var hoje = DateTime.Today;

        var acao = () => PeriodoLeitura.Criar(hoje, hoje.AddDays(1));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    [Fact]
    public void DiasDeLeitura_DeveContarOsDiasInclusive()
    {
        var hoje = DateTime.Today;

        var periodo = PeriodoLeitura.Criar(hoje.AddDays(-9), hoje);

        periodo.DiasDeLeitura.Should().Be(10);
    }

    [Fact]
    public void DiasDeLeitura_LeituraDeUmDia_DeveSerUm()
    {
        var hoje = DateTime.Today;

        PeriodoLeitura.Criar(hoje, hoje).DiasDeLeitura.Should().Be(1);
    }

    [Fact]
    public void AnoDeConclusao_DeveSerOAnoDaDataDeFim()
    {
        var periodo = PeriodoLeitura.Criar(
            new DateTime(2025, 12, 20), new DateTime(2026, 1, 10));

        periodo.AnoDeConclusao.Should().Be(2026);
    }
}

public class EmailTests
{
    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("ana.souza@example.com.br")]
    [InlineData("ana+livros@example.com")]
    public void Criar_ComEmailValido_DevePermitir(string valor)
        => Email.Criar(valor).Valor.Should().Be(valor.ToLowerInvariant());

    [Fact]
    public void Criar_DeveNormalizarParaMinusculas()
    {
        var email = Email.Criar("Ana.Souza@EXAMPLE.com");

        email.Valor.Should().Be("ana.souza@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComValorVazio_DeveLancarExcecao(string? valor)
    {
        var acao = () => Email.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*obrigatório*");
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("sem@dominio")]
    [InlineData("@example.com")]
    [InlineData("ana@")]
    public void Criar_ComFormatoInvalido_DeveLancarExcecao(string valor)
    {
        var acao = () => Email.Criar(valor);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não é válido*");
    }

    [Fact]
    public void Criar_AcimaDe150Caracteres_DeveLancarExcecao()
    {
        var longo = new string('a', 145) + "@example.com";

        var acao = () => Email.Criar(longo);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*150 caracteres*");
    }
}
