using FluentAssertions;
using Livros.Domain.Entities;
using Livros.Domain.Enums;
using Livros.Domain.Events;
using Livros.Domain.Exceptions;
using Xunit;

namespace Livros.UnitTests.Domain;

public class LivroTests
{
    private static Livro CriarLivro(
        string titulo = "Domain-Driven Design",
        string isbn = "978-0321125217",
        int paginas = 560,
        int ano = 2003)
        => new(
            titulo,
            "Tackling complexity in the heart of software.",
            isbn,
            "Eric Evans",
            "Addison-Wesley",
            GeneroLivroEnum.Tecnico,
            ano,
            paginas);

    // ----- Criação e validação de dados -----

    [Fact]
    public void Criar_ComDadosValidos_DevePreencherAsPropriedades()
    {
        var livro = CriarLivro();

        livro.Titulo.Should().Be("Domain-Driven Design");
        livro.Autor.Should().Be("Eric Evans");
        livro.IsbnValor.Should().Be("978-0321125217");
        livro.Genero.Should().Be(GeneroLivroEnum.Tecnico);
        livro.NotaMedia.Should().Be(0);
        livro.Avaliacoes.Should().BeEmpty();
        livro.TemCapa.Should().BeFalse();
    }

    [Fact]
    public void Criar_DeveNormalizarOIsbnParaComparacao()
    {
        var livro = CriarLivro(isbn: "978-0321125217");

        livro.IsbnNormalizado.Should().Be("9780321125217");
    }

    [Fact]
    public void Criar_DeveDispararEventoLivroCadastrado()
    {
        var livro = CriarLivro();

        livro.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LivroCadastradoEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComTituloVazio_DeveLancarExcecao(string titulo)
    {
        var acao = () => CriarLivro(titulo: titulo);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*título*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComPaginasInvalidas_DeveLancarExcecao(int paginas)
    {
        var acao = () => CriarLivro(paginas: paginas);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*páginas*");
    }

    [Theory]
    [InlineData(1400)]
    [InlineData(1200)]
    public void Criar_ComAnoAnteriorAoMinimo_DeveLancarExcecao(int ano)
    {
        var acao = () => CriarLivro(ano: ano);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*ano de publicação*");
    }

    [Fact]
    public void Criar_ComAnoMuitoNoFuturo_DeveLancarExcecao()
    {
        var acao = () => CriarLivro(ano: DateTime.Today.Year + 5);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*ano de publicação*");
    }

    [Fact]
    public void Criar_ComAnoMinimo_DevePermitir()
    {
        var acao = () => CriarLivro(ano: Livro.AnoMinimoPublicacao);

        acao.Should().NotThrow();
    }

    // ----- Atualização -----

    [Fact]
    public void Atualizar_DeveTrocarOsDadosEOIsbnNormalizado()
    {
        var livro = CriarLivro();

        livro.Atualizar(
            "Duna",
            "Arrakis.",
            "978-8576572008",
            "Frank Herbert",
            "Aleph",
            GeneroLivroEnum.FiccaoCientifica,
            1965,
            680);

        livro.Titulo.Should().Be("Duna");
        livro.IsbnNormalizado.Should().Be("9788576572008");
        livro.Genero.Should().Be(GeneroLivroEnum.FiccaoCientifica);
    }

    // ----- Capa do livro -----

    [Fact]
    public void DefinirCapa_ComBytesValidos_DeveArmazenar()
    {
        var livro = CriarLivro();
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        livro.DefinirCapa(bytes);

        livro.TemCapa.Should().BeTrue();
        livro.CapaLivro.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void DefinirCapa_ComArrayVazio_DeveLancarExcecao()
    {
        var livro = CriarLivro();

        var acao = () => livro.DefinirCapa(Array.Empty<byte>());

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*vazio*");
    }

    [Fact]
    public void DefinirCapa_AcimaDoTamanhoMaximo_DeveLancarExcecao()
    {
        var livro = CriarLivro();
        var grande = new byte[Livro.TamanhoMaximoCapaBytes + 1];

        var acao = () => livro.DefinirCapa(grande);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*no máximo*");
    }

    [Fact]
    public void RemoverCapa_DeveLimparOsBytes()
    {
        var livro = CriarLivro();
        livro.DefinirCapa(new byte[] { 0xFF, 0xD8, 0xFF });

        livro.RemoverCapa();

        livro.TemCapa.Should().BeFalse();
    }

    // ----- Avaliações e nota média -----

    [Fact]
    public void AdicionarAvaliacao_DeveAdicionarNaColecao()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        var avaliacao = livro.AdicionarAvaliacao(
            1, 5, "Excelente", hoje.AddDays(-30), hoje.AddDays(-10));

        avaliacao.Nota.Should().Be(5);
        livro.Avaliacoes.Should().ContainSingle();
        livro.TotalAvaliacoes.Should().Be(1);
    }

    [Fact]
    public void AdicionarAvaliacao_DeveDispararEventoAvaliacaoCadastrada()
    {
        var livro = CriarLivro();
        livro.LimparEventos();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 4, "Bom", hoje.AddDays(-20), hoje.AddDays(-5));

        livro.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AvaliacaoCadastradaEvent>();
    }

    [Fact]
    public void AdicionarAvaliacao_PeloMesmoUsuarioDuasVezes_DeveLancarExcecao()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 5, "Primeira", hoje.AddDays(-30), hoje.AddDays(-20));

        var acao = () => livro.AdicionarAvaliacao(1, 3, "Segunda", hoje.AddDays(-10), hoje);

        acao.Should().Throw<RegraNegocioException>()
            .WithMessage("*já avaliou*");
    }

    [Fact]
    public void AdicionarAvaliacao_DeUsuariosDiferentes_DevePermitir()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 5, "Ana", hoje.AddDays(-30), hoje.AddDays(-20));
        livro.AdicionarAvaliacao(2, 3, "Bruno", hoje.AddDays(-15), hoje.AddDays(-5));

        livro.Avaliacoes.Should().HaveCount(2);
    }

    // ----- Regra PLUS: nota média recalculada a cada avaliação -----

    [Fact]
    public void RecalcularNotaMedia_SemAvaliacoes_DeveSerZero()
    {
        var livro = CriarLivro();

        livro.RecalcularNotaMedia();

        livro.NotaMedia.Should().Be(0);
    }

    [Fact]
    public void RecalcularNotaMedia_ComUmaAvaliacao_DeveSerAPropriaNota()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 4, "Bom", hoje.AddDays(-20), hoje.AddDays(-5));
        livro.RecalcularNotaMedia();

        livro.NotaMedia.Should().Be(4m);
    }

    [Fact]
    public void RecalcularNotaMedia_ComVariasAvaliacoes_DeveSerAMedia()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 5, "Ana", hoje.AddDays(-60), hoje.AddDays(-50));
        livro.AdicionarAvaliacao(2, 4, "Bruno", hoje.AddDays(-40), hoje.AddDays(-30));
        livro.RecalcularNotaMedia();

        livro.NotaMedia.Should().Be(4.5m);
    }

    [Fact]
    public void RecalcularNotaMedia_DeveArredondarParaDuasCasas()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        // Média de 5, 4 e 4 = 4,333...
        livro.AdicionarAvaliacao(1, 5, "A", hoje.AddDays(-60), hoje.AddDays(-50));
        livro.AdicionarAvaliacao(2, 4, "B", hoje.AddDays(-40), hoje.AddDays(-30));
        livro.AdicionarAvaliacao(3, 4, "C", hoje.AddDays(-20), hoje.AddDays(-10));
        livro.RecalcularNotaMedia();

        livro.NotaMedia.Should().Be(4.33m);
    }

    [Fact]
    public void RecalcularNotaMedia_QuandoAMediaMuda_DeveDispararEvento()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;
        livro.AdicionarAvaliacao(1, 5, "Ana", hoje.AddDays(-30), hoje.AddDays(-20));
        livro.LimparEventos();

        livro.RecalcularNotaMedia();

        var evento = livro.DomainEvents.OfType<NotaMediaAtualizadaEvent>().Single();
        evento.NotaMediaAnterior.Should().Be(0m);
        evento.NotaMediaAtual.Should().Be(5m);
        evento.TotalAvaliacoes.Should().Be(1);
        evento.Variacao.Should().Be(5m);
    }

    [Fact]
    public void RecalcularNotaMedia_QuandoAMediaNaoMuda_NaoDeveDispararEvento()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;
        livro.AdicionarAvaliacao(1, 5, "Ana", hoje.AddDays(-30), hoje.AddDays(-20));
        livro.RecalcularNotaMedia();
        livro.LimparEventos();

        livro.RecalcularNotaMedia();

        livro.DomainEvents.Should().BeEmpty();
    }

    // ----- Distribuição de notas -----

    [Fact]
    public void DistribuicaoDeNotas_DeveContarQuantasAvaliacoesPorNota()
    {
        var livro = CriarLivro();
        var hoje = DateTime.Today;

        livro.AdicionarAvaliacao(1, 5, "A", hoje.AddDays(-60), hoje.AddDays(-50));
        livro.AdicionarAvaliacao(2, 5, "B", hoje.AddDays(-40), hoje.AddDays(-30));
        livro.AdicionarAvaliacao(3, 3, "C", hoje.AddDays(-20), hoje.AddDays(-10));

        var distribuicao = livro.DistribuicaoDeNotas;

        distribuicao[5].Should().Be(2);
        distribuicao[3].Should().Be(1);
        distribuicao[1].Should().Be(0);
        distribuicao.Should().HaveCount(5);
    }
}
