using Clinica.Api.Enums;
using Clinica.Api.Util;

namespace Clinica.Api.Entidades;

public class Paciente
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>CPF armazenado apenas com dígitos, para a busca funcionar.</summary>
    public string Cpf { get; set; } = string.Empty;

    /// <summary>Celular armazenado apenas com dígitos.</summary>
    public string Celular { get; set; } = string.Empty;

    public string? Email { get; set; }
    public DateTime DataNascimento { get; set; }
    public Sexo Sexo { get; set; }
    public string? Observacoes { get; set; }

    public List<Atendimento> Atendimentos { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool EstaDeletado { get; set; }

    public string CpfFormatado => Documentos.FormatarCpf(Cpf);
    public string CelularFormatado => Documentos.FormatarCelular(Celular);

    public int Idade
    {
        get
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade;
        }
    }

    public bool EhMenorDeIdade => Idade < 18;

    public IEnumerable<Atendimento> AtendimentosAtivos
        => Atendimentos.Where(a => !a.EstaDeletado);

    public override string ToString()
        => $"[{Id}] {NomeCompleto,-30} CPF {CpfFormatado}  {CelularFormatado}  {Idade} anos";
}
