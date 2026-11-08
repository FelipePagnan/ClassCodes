using BancoSangue.Console.Enums;

namespace BancoSangue.Console.Entidades;

public class Doador
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public Genero Genero { get; set; }
    public double Peso { get; set; }
    public TipoSanguineo TipoSanguineo { get; set; }
    public FatorRh FatorRh { get; set; }

    public List<Doacao> Doacoes { get; set; } = new();
    public Endereco? Endereco { get; set; }

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

    public DateTime? UltimaDoacao => Doacoes.Count == 0
        ? null
        : Doacoes.Max(d => d.DataDoacao);

    /// <summary>
    /// Intervalo mínimo exigido entre doações, de acordo com o gênero do doador (regra PLUS).
    /// </summary>
    public int IntervaloMinimoDiasEntreDoacoes => Genero switch
    {
        Genero.Feminino => 90,
        _ => 60
    };

    public string TipoSanguineoCompleto => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    public override string ToString()
        => $"[{Id}] {NomeCompleto} - {Email} - {TipoSanguineoCompleto} - {Idade} anos";
}
