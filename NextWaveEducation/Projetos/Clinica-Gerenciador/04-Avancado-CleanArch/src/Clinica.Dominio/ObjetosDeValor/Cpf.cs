using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Servicos;

namespace Clinica.Dominio.ObjetosDeValor;

/// <summary>
/// CPF como objeto de valor. A diferença em relação a guardar uma string é
/// que um Cpf só existe se for válido: a verificação acontece na criação, e
/// não em cada lugar que usa o dado.
///
/// É um record, então a comparação é por valor — dois Cpf com os mesmos
/// dígitos são iguais.
/// </summary>
public record Cpf
{
    private Cpf(string valor) => Valor = valor;

    /// <summary>Somente os 11 dígitos, que é a forma como o banco guarda.</summary>
    public string Valor { get; }

    /// <summary>Com pontuação, para exibir: 529.982.247-25.</summary>
    public string Formatado => Documentos.FormatarCpf(Valor);

    public static Cpf Criar(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            throw new RegraNegocioException("O CPF é obrigatório.");

        if (!Documentos.CpfValido(entrada))
            throw new RegraNegocioException(
                $"O CPF '{entrada}' é inválido (confira os dígitos verificadores).");

        return new Cpf(Documentos.SomenteDigitos(entrada));
    }

    /// <summary>
    /// Reconstrói sem validar. Usado só pelo Entity Framework ao ler do
    /// banco: o dado já foi validado quando entrou.
    /// </summary>
    public static Cpf Reconstruir(string valor) => new(valor);

    public static bool EhValido(string? entrada) => Documentos.CpfValido(entrada ?? string.Empty);

    public override string ToString() => Formatado;
}
