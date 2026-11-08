using Clinica.Api.Util;

namespace Clinica.Api.Entidades;

public class Medico
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>Número do CRM, apenas dígitos.</summary>
    public string Crm { get; set; } = string.Empty;

    /// <summary>UF do conselho que emitiu o CRM.</summary>
    public string UfCrm { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }

    public List<Atendimento> Atendimentos { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool EstaDeletado { get; set; }

    /// <summary>Identificação profissional completa, ex: CRM/SP 123456.</summary>
    public string CrmCompleto => $"CRM/{UfCrm} {Crm}";

    public string? CelularFormatado => string.IsNullOrWhiteSpace(Celular)
        ? null
        : Documentos.FormatarCelular(Celular);

    public IEnumerable<Atendimento> AtendimentosAtivos
        => Atendimentos.Where(a => !a.EstaDeletado);

    public override string ToString()
        => $"[{Id}] {NomeCompleto,-28} {CrmCompleto,-16} {Especialidade}";
}
