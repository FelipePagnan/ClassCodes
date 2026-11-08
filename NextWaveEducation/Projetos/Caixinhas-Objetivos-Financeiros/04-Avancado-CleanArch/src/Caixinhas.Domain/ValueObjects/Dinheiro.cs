using Caixinhas.Domain.Exceptions;

namespace Caixinhas.Domain.ValueObjects;

/// <summary>
/// Value Object de valor monetário. É aqui que mora a regra "não pode ser
/// negativo e tem no máximo duas casas decimais": um valor inválido
/// simplesmente não consegue existir no domínio.
/// </summary>
public sealed record Dinheiro : IComparable<Dinheiro>
{
    /// <summary>Número máximo de casas decimais aceito.</summary>
    public const int CasasDecimais = 2;

    /// <summary>Teto de sanidade para evitar valores absurdos.</summary>
    public const decimal ValorMaximo = 999_999_999.99m;

    private Dinheiro(decimal valor) => Valor = valor;

    public decimal Valor { get; }

    public static Dinheiro Zero { get; } = new(0m);

    /// <summary>
    /// Cria um valor monetário validando as regras de negócio.
    /// <paramref name="nomeDoCampo"/> deixa a mensagem de erro específica.
    /// </summary>
    public static Dinheiro Criar(decimal valor, string nomeDoCampo = "valor")
    {
        // Regra de negócio: não pode ser negativo.
        if (valor < 0)
            throw new RegraNegocioException(
                $"O {nomeDoCampo} não pode ser negativo. Valor informado: {valor}.");

        // Regra de negócio: até duas casas decimais de precisão.
        // decimal.Round compara por valor, então 10.500 é aceito (vale 10.50)
        // e 10.555 é recusado.
        if (decimal.Round(valor, CasasDecimais) != valor)
            throw new RegraNegocioException(
                $"O {nomeDoCampo} deve ter no máximo {CasasDecimais} casas decimais. " +
                $"Valor informado: {valor}.");

        if (valor > ValorMaximo)
            throw new RegraNegocioException(
                $"O {nomeDoCampo} excede o limite de {ValorMaximo:N2}.");

        return new Dinheiro(valor);
    }

    /// <summary>Cria um valor que também precisa ser maior que zero.</summary>
    public static Dinheiro CriarPositivo(decimal valor, string nomeDoCampo = "valor")
    {
        var dinheiro = Criar(valor, nomeDoCampo);

        if (dinheiro.Valor == 0)
            throw new RegraNegocioException($"O {nomeDoCampo} deve ser maior que zero.");

        return dinheiro;
    }

    /// <summary>Verifica as regras sem lançar exceção. Útil nos validadores.</summary>
    public static bool EhValido(decimal valor)
        => valor >= 0
        && decimal.Round(valor, CasasDecimais) == valor
        && valor <= ValorMaximo;

    public Dinheiro Somar(Dinheiro outro) => Criar(Valor + outro.Valor);

    public Dinheiro Subtrair(Dinheiro outro)
    {
        var resultado = Valor - outro.Valor;

        if (resultado < 0)
            throw new RegraNegocioException(
                $"A operação resultaria em valor negativo: {resultado:N2}.");

        return Criar(resultado);
    }

    public bool EhZero => Valor == 0;

    public int CompareTo(Dinheiro? outro) => outro is null ? 1 : Valor.CompareTo(outro.Valor);

    public override string ToString() => Valor.ToString("N2");

    public static implicit operator decimal(Dinheiro dinheiro) => dinheiro.Valor;

    public static bool operator >(Dinheiro a, Dinheiro b) => a.Valor > b.Valor;
    public static bool operator <(Dinheiro a, Dinheiro b) => a.Valor < b.Valor;
    public static bool operator >=(Dinheiro a, Dinheiro b) => a.Valor >= b.Valor;
    public static bool operator <=(Dinheiro a, Dinheiro b) => a.Valor <= b.Valor;
}
