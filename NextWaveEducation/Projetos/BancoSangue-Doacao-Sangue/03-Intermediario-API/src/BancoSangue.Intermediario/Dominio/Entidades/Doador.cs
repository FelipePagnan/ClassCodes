using BancoSangue.Intermediario.Dominio.Enums;
using BancoSangue.Intermediario.Dominio.Excecoes;

namespace BancoSangue.Intermediario.Dominio.Entidades;

public class Doador : EntidadeBase
{
    public const double PesoMinimoKg = 50;
    public const int IntervaloDiasMasculino = 60;
    public const int IntervaloDiasFeminino = 90;

    // Construtor usado pelo EF Core.
    protected Doador() { }

    public Doador(
        string nomeCompleto,
        string email,
        DateTime dataNascimento,
        Genero genero,
        double peso,
        TipoSanguineo tipoSanguineo,
        FatorRh fatorRh)
    {
        NomeCompleto = nomeCompleto;
        Email = email;
        DataNascimento = dataNascimento;
        Genero = genero;
        Peso = peso;
        TipoSanguineo = tipoSanguineo;
        FatorRh = fatorRh;
        Doacoes = new List<Doacao>();
    }

    public string NomeCompleto { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public Genero Genero { get; private set; }
    public double Peso { get; private set; }
    public TipoSanguineo TipoSanguineo { get; private set; }
    public FatorRh FatorRh { get; private set; }

    public List<Doacao> Doacoes { get; private set; } = new();
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

    public bool EhMaiorDeIdade => Idade >= 18;

    public string TipoSanguineoCompleto => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    public DateTime? UltimaDoacao => Doacoes.Count == 0 ? null : Doacoes.Max(d => d.DataDoacao);

    /// <summary>
    /// Intervalo mínimo em dias entre doações: 60 dias para homens, 90 dias para mulheres.
    /// </summary>
    public int IntervaloMinimoDias => Genero == Genero.Feminino
        ? IntervaloDiasFeminino
        : IntervaloDiasMasculino;

    public void DefinirEndereco(Endereco endereco)
    {
        Endereco = endereco;
        endereco.VincularDoador(this);
    }

    public void AtualizarPeso(double peso)
    {
        if (peso <= 0)
            throw new RegraNegocioException("O peso deve ser maior que zero.");
        Peso = peso;
    }

    /// <summary>
    /// Verifica todas as regras de negócio que autorizam (ou não) este doador a doar na data informada.
    /// Lança RegraNegocioException quando alguma regra não é atendida.
    /// </summary>
    public void GarantirQuePodeDoar(DateTime dataDoacao)
    {
        if (!EhMaiorDeIdade)
            throw new RegraNegocioException($"'{NomeCompleto}' é menor de idade e não pode doar sangue.");

        if (Peso < PesoMinimoKg)
            throw new RegraNegocioException(
                $"'{NomeCompleto}' pesa {Peso}kg. O peso mínimo para doar é {PesoMinimoKg}kg.");

        if (UltimaDoacao is DateTime ultima)
        {
            var dias = (dataDoacao.Date - ultima.Date).TotalDays;
            if (dias < IntervaloMinimoDias)
            {
                var restantes = IntervaloMinimoDias - dias;
                throw new RegraNegocioException(
                    $"'{NomeCompleto}' deve aguardar mais {restantes:0} dia(s) para doar novamente " +
                    $"(intervalo mínimo de {IntervaloMinimoDias} dias).");
            }
        }
    }

    public Doacao RegistrarDoacao(int quantidadeML, DateTime dataDoacao)
    {
        GarantirQuePodeDoar(dataDoacao);

        var doacao = new Doacao(Id, quantidadeML, dataDoacao);
        Doacoes.Add(doacao);
        return doacao;
    }
}
