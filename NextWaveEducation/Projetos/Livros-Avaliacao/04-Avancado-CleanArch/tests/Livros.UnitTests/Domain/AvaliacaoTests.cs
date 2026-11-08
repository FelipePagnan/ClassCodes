using FluentAssertions;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Exceptions;
using Xunit;

namespace Livros.UnitTests.Domain;

public class AvaliacaoTests
{
    private static Livro CriarLivro(int paginas = 300) => new(
        "Livro de Teste",
        "Descrição",
        "978-0321125217",
        "Autor Teste",
        "Editora Teste",
        GeneroLivroEnum.Ficcao,
        2020,
        paginas);

    // ----- Regra: a nota deve ser de 1 a 5 -----

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AdicionarAvaliacao_ComNotaNaFaixaPermitida_DeveFuncionar(int nota)
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var avaliacao = livro.AdicionarAvaliacao(
            1, nota, "Comentário", hoje.AddDays(-20), hoje.AddDays(-5));

        avaliacao.Nota.Should().Be(nota);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public void AdicionarAvaliacao_ComNotaForaDaFaixa_DeveLancarExcecao(int nota)
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var acao = () => livro.AdicionarAvaliacao(
            1, nota, "Comentário", hoje.AddDays(-20), hoje.AddDays(-5));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*nota deve ser de 1 a 5*");
    }

    [Fact]
    public void LimitesDeNota_DevemSeguirAEspecificacao()
    {
        Avaliacao.NotaMinima.Should().Be(1);
        Avaliacao.NotaMaxima.Should().Be(5);
    }

    // ----- Regra: data de início não pode ser maior que a de fim -----

    [Fact]
    public void AdicionarAvaliacao_ComInicioMaiorQueOFim_DeveLancarExcecao()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var acao = () => livro.AdicionarAvaliacao(
            1, 4, "Comentário", hoje.AddDays(-5), hoje.AddDays(-20));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*não pode ser maior que a data de fim*");
    }

    [Fact]
    public void AdicionarAvaliacao_ComDatasIguais_DevePermitir()
    {
        var livro = CriarLivro();
        var ontem = DateTime.Today.AddDays(-1);

        var acao = () => livro.AdicionarAvaliacao(1, 4, "Leu num dia", ontem, ontem);

        acao.Should().NotThrow();
    }

    [Fact]
    public void AdicionarAvaliacao_ComFimNoFuturo_DeveLancarExcecao()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var acao = () => livro.AdicionarAvaliacao(
            1, 4, "Comentário", hoje, hoje.AddDays(10));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*futuro*");
    }

    // ----- Comentário -----

    [Fact]
    public void AdicionarAvaliacao_ComComentarioAcimaDoLimite_DeveLancarExcecao()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;
        var longo = new string('a', 2001);

        var acao = () => livro.AdicionarAvaliacao(
            1, 4, longo, hoje.AddDays(-20), hoje.AddDays(-5));

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*2000 caracteres*");
    }

    [Fact]
    public void AdicionarAvaliacao_SemComentario_DevePermitir()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var avaliacao = livro.AdicionarAvaliacao(
            1, 5, string.Empty, hoje.AddDays(-20), hoje.AddDays(-5));

        avaliacao.Descricao.Should().BeEmpty();
    }

    // ----- Propriedades calculadas -----

    [Fact]
    public void DiasDeLeitura_DeveContarOsDiasInclusive()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var avaliacao = livro.AdicionarAvaliacao(
            1, 4, "Comentário", hoje.AddDays(-9), hoje);

        avaliacao.DiasDeLeitura.Should().Be(10);
    }

    [Fact]
    public void AnoDeConclusao_DeveSerOAnoDaDataDeFim()
    {
        var livro = CriarLivro();

        var avaliacao = livro.AdicionarAvaliacao(
            1, 4, "Comentário",
            new DateTime(2025, 12, 20), new DateTime(2026, 1, 10));

        avaliacao.AnoDeConclusao.Should().Be(2026);
    }

    [Fact]
    public void Periodo_DeveReconstruirOValueObject()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var avaliacao = livro.AdicionarAvaliacao(
            1, 4, "Comentário", hoje.AddDays(-20), hoje.AddDays(-5));

        avaliacao.Periodo.Inicio.Should().Be(hoje.AddDays(-20));
        avaliacao.Periodo.Fim.Should().Be(hoje.AddDays(-5));
    }
}
