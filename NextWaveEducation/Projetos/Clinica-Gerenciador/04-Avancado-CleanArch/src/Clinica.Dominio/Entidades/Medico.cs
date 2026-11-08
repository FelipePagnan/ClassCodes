using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;

namespace Clinica.Dominio.Entidades;

/// <summary>Médico da clínica, identificado pelo par CRM + UF.</summary>
public class Medico : EntidadeBase
{
    private readonly List<Atendimento> _atendimentos = new();

    private Medico() { }

    public Medico(
        string nomeCompleto,
        Crm crm,
        string especialidade,
        Email? email = null,
        Celular? celular = null)
    {
        AlterarDados(nomeCompleto, crm, especialidade, email, celular);
        DataCriacao = DateTime.Now;
    }

    public string NomeCompleto { get; private set; } = string.Empty;
    public Crm Crm { get; private set; } = null!;
    public string Especialidade { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public Celular? Celular { get; private set; }

    public IReadOnlyCollection<Atendimento> Atendimentos => _atendimentos.AsReadOnly();

    public void AlterarDados(
        string nomeCompleto,
        Crm crm,
        string especialidade,
        Email? email,
        Celular? celular)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto) || nomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(especialidade))
            throw new RegraNegocioException("A especialidade é obrigatória.");

        if (especialidade.Trim().Length > 120)
            throw new RegraNegocioException("A especialidade deve ter no máximo 120 caracteres.");

        NomeCompleto = nomeCompleto.Trim();
        Crm = crm;
        Especialidade = especialidade.Trim();
        Email = email;
        Celular = celular;
    }
}
