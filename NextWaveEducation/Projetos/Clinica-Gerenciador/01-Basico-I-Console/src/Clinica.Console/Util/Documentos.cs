namespace Clinica.Console.Util;

/// <summary>
/// Validação e formatação de CPF e celular. O CPF é conferido pelos dígitos
/// verificadores, não apenas pela quantidade de dígitos: num sistema de
/// clínica a busca é feita por CPF, então um dígito errado significa paciente
/// que nunca mais será encontrado.
/// </summary>
public static class Documentos
{
    /// <summary>Mantém apenas os dígitos de uma string.</summary>
    public static string SomenteDigitos(string? valor)
        => string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());

    // ---------------------------------- CPF ----------------------------------

    /// <summary>
    /// Confere os dois dígitos verificadores do CPF.
    /// </summary>
    public static bool CpfValido(string? cpf)
    {
        var digitos = SomenteDigitos(cpf);

        if (digitos.Length != 11) return false;

        // Sequências repetidas (00000000000, 11111111111...) passam no cálculo
        // dos dígitos mas não são CPFs válidos.
        if (digitos.All(c => c == digitos[0])) return false;

        var primeiroDigito = CalcularDigito(digitos[..9], 10);
        if (digitos[9] != primeiroDigito) return false;

        var segundoDigito = CalcularDigito(digitos[..10], 11);
        return digitos[10] == segundoDigito;
    }

    /// <summary>
    /// Calcula um dígito verificador: cada algarismo é multiplicado por um
    /// peso decrescente, soma-se tudo e tira-se o resto por 11.
    /// </summary>
    private static char CalcularDigito(string parcial, int pesoInicial)
    {
        var soma = 0;
        var peso = pesoInicial;

        foreach (var algarismo in parcial)
        {
            soma += (algarismo - '0') * peso;
            peso--;
        }

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;

        return (char)('0' + digito);
    }

    /// <summary>Formata o CPF como 000.000.000-00.</summary>
    public static string FormatarCpf(string cpf)
    {
        var d = SomenteDigitos(cpf);
        return d.Length != 11 ? cpf : $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
    }

    // -------------------------------- CELULAR --------------------------------

    private static readonly int[] _dddsValidos =
    [
        11, 12, 13, 14, 15, 16, 17, 18, 19,
        21, 22, 24, 27, 28,
        31, 32, 33, 34, 35, 37, 38,
        41, 42, 43, 44, 45, 46, 47, 48, 49,
        51, 53, 54, 55,
        61, 62, 63, 64, 65, 66, 67, 68, 69,
        71, 73, 74, 75, 77, 79,
        81, 82, 83, 84, 85, 86, 87, 88, 89,
        91, 92, 93, 94, 95, 96, 97, 98, 99
    ];

    /// <summary>
    /// Valida um celular brasileiro: 11 dígitos, DDD existente e nono dígito
    /// começando com 9 (padrão de celular desde 2016).
    /// </summary>
    public static bool CelularValido(string? celular)
    {
        var digitos = SomenteDigitos(celular);

        if (digitos.Length != 11) return false;

        var ddd = int.Parse(digitos[..2]);
        if (!_dddsValidos.Contains(ddd)) return false;

        return digitos[2] == '9';
    }

    /// <summary>Formata o celular como (00) 90000-0000.</summary>
    public static string FormatarCelular(string celular)
    {
        var d = SomenteDigitos(celular);
        return d.Length != 11 ? celular : $"({d[..2]}) {d[2..7]}-{d[7..]}";
    }

    // ---------------------------------- CRM ----------------------------------

    private static readonly string[] _ufs =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS",
        "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC",
        "SP", "SE", "TO"
    ];

    public static bool UfValida(string? uf)
        => !string.IsNullOrWhiteSpace(uf) && _ufs.Contains(uf.Trim().ToUpperInvariant());

    public static IReadOnlyList<string> Ufs => _ufs;

    /// <summary>O CRM é numérico, de 4 a 7 dígitos dependendo do conselho.</summary>
    public static bool CrmValido(string? crm)
    {
        var digitos = SomenteDigitos(crm);
        return digitos.Length is >= 4 and <= 7;
    }
}
