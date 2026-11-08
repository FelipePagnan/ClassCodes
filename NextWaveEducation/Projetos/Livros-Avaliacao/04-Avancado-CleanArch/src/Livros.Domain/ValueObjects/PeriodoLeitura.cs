using Livros.Domain.Exceptions;

namespace Livros.Domain.ValueObjects;

/// <summary>
/// Value Object do período de leitura. É aqui que mora a regra
/// "a data de início não pode ser maior que a data de fim".
/// </summary>
public sealed record PeriodoLeitura
{
    private PeriodoLeitura(DateTime inicio, DateTime fim)
    {
        Inicio = inicio;
        Fim = fim;
    }

    public DateTime Inicio { get; }
    public DateTime Fim { get; }

    /// <summary>Quantidade de dias que a leitura levou (inclusive).</summary>
    public int DiasDeLeitura => (Fim.Date - Inicio.Date).Days + 1;

    /// <summary>Ano em que a leitura foi concluída. Base do relatório anual.</summary>
    public int AnoDeConclusao => Fim.Year;

    public static PeriodoLeitura Criar(DateTime inicio, DateTime fim)
    {
        // Regra de negócio: a data de início não pode ser maior que a de fim.
        if (inicio.Date > fim.Date)
            throw new RegraNegocioException(
                $"A data de início da leitura ({inicio:dd/MM/yyyy}) não pode ser " +
                $"maior que a data de fim ({fim:dd/MM/yyyy}).");

        if (fim.Date > DateTime.Today)
            throw new RegraNegocioException(
                $"A data de fim da leitura ({fim:dd/MM/yyyy}) não pode estar no futuro.");

        if (inicio.Year < 1900)
            throw new RegraNegocioException("A data de início da leitura é inválida.");

        return new PeriodoLeitura(inicio.Date, fim.Date);
    }

    public override string ToString() => $"{Inicio:dd/MM/yyyy} a {Fim:dd/MM/yyyy}";
}
