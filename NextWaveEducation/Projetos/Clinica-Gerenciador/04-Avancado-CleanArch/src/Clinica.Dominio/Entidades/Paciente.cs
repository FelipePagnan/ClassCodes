using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;

namespace Clinica.Dominio.Entidades;

/// <summary>
/// Paciente da clínica. Identificado pelo CPF, encontrado pelo CPF ou pelo
/// celular.
/// </summary>
public class Paciente : EntidadeBase
{
    private readonly List<Atendimento> _atendimentos = new();

    /// <summary>Construtor sem parâmetros exigido pelo Entity Framework.</summary>
    private Paciente() { }

    public Paciente(
        string nomeCompleto,
        Cpf cpf,
        Celular celular,
        DateTime dataNascimento,
        Sexo sexo,
        Email? email = null,
        string? observacoes = null)
    {
        AlterarDados(nomeCompleto, cpf, celular, dataNascimento, sexo, email, observacoes);
        DataCriacao = DateTime.Now;
    }

    public string NomeCompleto { get; private set; } = string.Empty;
    public Cpf Cpf { get; private set; } = null!;
    public Celular Celular { get; private set; } = null!;
    public Email? Email { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public Sexo Sexo { get; private set; }
    public string? Observacoes { get; private set; }

    /// <summary>
    /// Só leitura para fora: atendimento se cria pelo AtendimentoService, não
    /// empurrando item nesta lista.
    /// </summary>
    public IReadOnlyCollection<Atendimento> Atendimentos => _atendimentos.AsReadOnly();

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

    public void AlterarDados(
        string nomeCompleto,
        Cpf cpf,
        Celular celular,
        DateTime dataNascimento,
        Sexo sexo,
        Email? email,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto) || nomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (nomeCompleto.Trim().Length > 200)
            throw new RegraNegocioException("O nome completo deve ter no máximo 200 caracteres.");

        if (dataNascimento == default)
            throw new RegraNegocioException("A data de nascimento é obrigatória.");

        if (dataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("A data de nascimento não pode estar no futuro.");

        if (dataNascimento.Year < 1900)
            throw new RegraNegocioException("A data de nascimento informada é inválida.");

        if (!Enum.IsDefined(sexo))
            throw new RegraNegocioException($"Sexo '{sexo}' inválido.");

        NomeCompleto = nomeCompleto.Trim();
        Cpf = cpf;
        Celular = celular;
        Email = email;
        DataNascimento = dataNascimento.Date;
        Sexo = sexo;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
    }

    /// <summary>Primeiro nome, para abrir as mensagens de confirmação.</summary>
    public string PrimeiroNome => NomeCompleto.Split(' ')[0];
}
