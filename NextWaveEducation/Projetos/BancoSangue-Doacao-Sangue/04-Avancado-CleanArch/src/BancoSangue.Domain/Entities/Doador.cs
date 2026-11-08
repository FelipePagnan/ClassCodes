using BancoSangue.Domain.Common;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Events;
using BancoSangue.Domain.Exceptions;
using BancoSangue.Domain.ValueObjects;

namespace BancoSangue.Domain.Entities;

/// <summary>
/// Raiz de agregação do doador. Concentra todas as regras de negócio
/// que determinam se um doador pode ou não doar sangue.
/// </summary>
public class Doador : BaseEntity
{
    /// <summary>Peso mínimo exigido para doar sangue, em quilos.</summary>
    public const double PesoMinimoKg = 50;

    /// <summary>Idade mínima para doar sangue.</summary>
    public const int IdadeMinimaParaDoar = 18;

    /// <summary>Intervalo mínimo entre doações para homens, em dias.</summary>
    public const int IntervaloDiasMasculino = 60;

    /// <summary>Intervalo mínimo entre doações para mulheres, em dias.</summary>
    public const int IntervaloDiasFeminino = 90;

    private readonly List<Doacao> _doacoes = new();

    // Construtor sem parâmetros para o EF Core.
    private Doador() { }

    public Doador(
        string nomeCompleto,
        string email,
        DateTime dataNascimento,
        Genero genero,
        double peso,
        TipoSanguineo tipoSanguineo,
        FatorRh fatorRh)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto) || nomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (dataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("A data de nascimento não pode estar no futuro.");

        if (peso <= 0)
            throw new RegraNegocioException("O peso deve ser maior que zero.");

        NomeCompleto = nomeCompleto.Trim();
        EmailValor = Email.Criar(email).Valor;
        DataNascimento = dataNascimento.Date;
        Genero = genero;
        Peso = peso;
        TipoSanguineo = tipoSanguineo;
        FatorRh = fatorRh;

        // Menor de idade PODE ter cadastro; apenas não pode doar.
        AdicionarEvento(new DoadorCadastradoEvent(Id, NomeCompleto, EmailValor, EstaAptoParaDoar));
    }

    public string NomeCompleto { get; private set; } = string.Empty;

    /// <summary>E-mail persistido. Exposto como Value Object através de <see cref="Email"/>.</summary>
    public string EmailValor { get; private set; } = string.Empty;

    public Email Email => ValueObjects.Email.Criar(EmailValor);

    public DateTime DataNascimento { get; private set; }
    public Genero Genero { get; private set; }
    public double Peso { get; private set; }
    public TipoSanguineo TipoSanguineo { get; private set; }
    public FatorRh FatorRh { get; private set; }

    public IReadOnlyCollection<Doacao> Doacoes => _doacoes.AsReadOnly();
    public Endereco? Endereco { get; private set; }

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

    public bool EhMaiorDeIdade => Idade >= IdadeMinimaParaDoar;

    public string TipoSanguineoCompleto => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    public DateTime? UltimaDoacao => _doacoes.Count == 0 ? null : _doacoes.Max(d => d.DataDoacao);

    /// <summary>
    /// Intervalo mínimo entre doações: 60 dias para homens, 90 dias para mulheres.
    /// </summary>
    public int IntervaloMinimoDias => Genero == Genero.Feminino
        ? IntervaloDiasFeminino
        : IntervaloDiasMasculino;

    public DateTime? ProximaDoacaoPermitida => UltimaDoacao?.AddDays(IntervaloMinimoDias);

    /// <summary>Atende aos requisitos estruturais (idade e peso), ignorando o intervalo.</summary>
    public bool EstaAptoParaDoar => EhMaiorDeIdade && Peso >= PesoMinimoKg;

    public void DefinirEndereco(Endereco endereco)
    {
        Endereco = endereco ?? throw new RegraNegocioException("O endereço informado é inválido.");
        endereco.VincularDoador(this);
    }

    public void AtualizarPeso(double novoPeso)
    {
        if (novoPeso <= 0)
            throw new RegraNegocioException("O peso deve ser maior que zero.");
        Peso = novoPeso;
    }

    /// <summary>
    /// Valida todas as regras que autorizam a doação na data informada.
    /// Lança <see cref="RegraNegocioException"/> quando alguma regra não é atendida.
    /// </summary>
    public void GarantirQuePodeDoar(DateTime dataDoacao)
    {
        if (!EhMaiorDeIdade)
            throw new RegraNegocioException(
                $"'{NomeCompleto}' tem {Idade} anos. Menores de idade não podem doar sangue.");

        if (Peso < PesoMinimoKg)
            throw new RegraNegocioException(
                $"'{NomeCompleto}' pesa {Peso}kg. O peso mínimo para doar é {PesoMinimoKg}kg.");

        if (UltimaDoacao is DateTime ultima)
        {
            var diasDesdeUltima = (dataDoacao.Date - ultima.Date).TotalDays;
            if (diasDesdeUltima < IntervaloMinimoDias)
            {
                var restantes = Math.Ceiling(IntervaloMinimoDias - diasDesdeUltima);
                throw new RegraNegocioException(
                    $"'{NomeCompleto}' doou em {ultima:dd/MM/yyyy}. " +
                    $"Faltam {restantes:0} dia(s) para completar o intervalo mínimo de {IntervaloMinimoDias} dias " +
                    $"({(Genero == Genero.Feminino ? "mulheres" : "homens")}).");
            }
        }
    }

    /// <summary>
    /// Registra uma doação para este doador, aplicando as regras de negócio
    /// e disparando o evento de domínio que atualiza o estoque.
    /// </summary>
    public Doacao RegistrarDoacao(int quantidadeML, DateTime dataDoacao)
    {
        GarantirQuePodeDoar(dataDoacao);

        var doacao = new Doacao(Id, quantidadeML, dataDoacao);
        _doacoes.Add(doacao);

        AdicionarEvento(new DoacaoRegistradaEvent(
            Id, NomeCompleto, TipoSanguineo, FatorRh, quantidadeML, dataDoacao));

        return doacao;
    }
}
