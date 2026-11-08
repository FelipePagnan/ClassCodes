using Livros.Domain.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.ValueObjects;

namespace Livros.Domain.Entities;

public class Avaliacao : BaseEntity
{
    /// <summary>Nota mínima permitida numa avaliação.</summary>
    public const int NotaMinima = 1;

    /// <summary>Nota máxima permitida numa avaliação.</summary>
    public const int NotaMaxima = 5;

    // Construtor sem parâmetros para o EF Core.
    private Avaliacao() { }

    internal Avaliacao(
        int idLivro,
        int idUsuario,
        int nota,
        string descricao,
        PeriodoLeitura periodo)
    {
        // Regra de negócio: a nota deve ser de 1 a 5.
        if (nota < NotaMinima || nota > NotaMaxima)
            throw new RegraNegocioException(
                $"A nota deve ser de {NotaMinima} a {NotaMaxima}. Valor informado: {nota}.");

        if (descricao is { Length: > 2000 })
            throw new RegraNegocioException("O comentário deve ter no máximo 2000 caracteres.");

        IdLivro = idLivro;
        IdUsuario = idUsuario;
        Nota = nota;
        Descricao = descricao?.Trim() ?? string.Empty;
        DataInicioLeitura = periodo.Inicio;
        DataFimLeitura = periodo.Fim;
        DataCriacao = DateTime.Now;
    }

    public int Nota { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public int IdUsuario { get; private set; }
    public int IdLivro { get; private set; }

    public DateTime DataInicioLeitura { get; private set; }
    public DateTime DataFimLeitura { get; private set; }

    public Livro Livro { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    /// <summary>Reconstrói o Value Object a partir das colunas persistidas.</summary>
    public PeriodoLeitura Periodo => PeriodoLeitura.Criar(DataInicioLeitura, DataFimLeitura);

    public int DiasDeLeitura => (DataFimLeitura.Date - DataInicioLeitura.Date).Days + 1;

    /// <summary>Ano em que a leitura foi concluída. Base do relatório anual.</summary>
    public int AnoDeConclusao => DataFimLeitura.Year;

    /// <summary>Páginas lidas por dia, útil em estatísticas do usuário.</summary>
    public double? PaginasPorDia => Livro is null || DiasDeLeitura <= 0
        ? null
        : Math.Round((double)Livro.QuantidadePaginas / DiasDeLeitura, 1);
}
