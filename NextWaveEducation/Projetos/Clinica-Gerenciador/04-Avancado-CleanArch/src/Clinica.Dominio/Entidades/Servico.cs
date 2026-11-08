using Clinica.Dominio.Excecoes;

namespace Clinica.Dominio.Entidades;

/// <summary>
/// Procedimento oferecido pela clínica. A duração é o que permite calcular
/// conflito de agenda.
/// </summary>
public class Servico : EntidadeBase
{
    /// <summary>Teto de duração: 8 horas.</summary>
    public const int DuracaoMaximaMinutos = 480;

    private readonly List<Atendimento> _atendimentos = new();

    private Servico() { }

    public Servico(string nome, decimal valor, int duracaoMinutos, string? descricao = null)
    {
        AlterarDados(nome, valor, duracaoMinutos, descricao);
        DataCriacao = DateTime.Now;
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public int DuracaoMinutos { get; private set; }

    public IReadOnlyCollection<Atendimento> Atendimentos => _atendimentos.AsReadOnly();

    public TimeSpan Duracao => TimeSpan.FromMinutes(DuracaoMinutos);

    public void AlterarDados(string nome, decimal valor, int duracaoMinutos, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do serviço deve ter ao menos 3 caracteres.");

        if (nome.Trim().Length > 150)
            throw new RegraNegocioException("O nome do serviço deve ter no máximo 150 caracteres.");

        if (valor < 0)
            throw new RegraNegocioException("O valor do serviço não pode ser negativo.");

        // decimal.Round compara por valor, não por escala: 250.00 passa,
        // 250.005 não.
        if (decimal.Round(valor, 2) != valor)
            throw new RegraNegocioException("O valor deve ter no máximo 2 casas decimais.");

        if (duracaoMinutos <= 0)
            throw new RegraNegocioException("A duração deve ser maior que zero.");

        if (duracaoMinutos > DuracaoMaximaMinutos)
            throw new RegraNegocioException(
                $"A duração não pode passar de {DuracaoMaximaMinutos} minutos.");

        Nome = nome.Trim();
        Valor = valor;
        DuracaoMinutos = duracaoMinutos;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
    }
}
