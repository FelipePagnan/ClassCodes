namespace Clinica.Api.Entidades;

/// <summary>
/// Um procedimento oferecido pela clínica. A duração é o que permite
/// detectar conflito de agenda.
/// </summary>
public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }

    /// <summary>Duração prevista em minutos. Usada no cálculo de conflito.</summary>
    public int DuracaoMinutos { get; set; }

    public List<Atendimento> Atendimentos { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool EstaDeletado { get; set; }

    public TimeSpan Duracao => TimeSpan.FromMinutes(DuracaoMinutos);

    public override string ToString()
        => $"[{Id}] {Nome,-30} R$ {Valor,10:N2}  {DuracaoMinutos} min";
}
