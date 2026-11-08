using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Servicos;

namespace Clinica.Dominio.ObjetosDeValor;

/// <summary>
/// Registro profissional do médico. Número e UF andam juntos porque o CRM só
/// é único dentro do estado que o emitiu — existe CRM/SP 123456 e CRM/RJ
/// 123456, de dois médicos diferentes.
/// </summary>
public record Crm
{
    private Crm(string numero, string uf)
    {
        Numero = numero;
        Uf = uf;
    }

    public string Numero { get; }
    public string Uf { get; }

    /// <summary>Identificação completa, ex: CRM/SP 123456.</summary>
    public string Completo => $"CRM/{Uf} {Numero}";

    public static Crm Criar(string? numero, string? uf)
    {
        if (!Documentos.CrmValido(numero ?? string.Empty))
            throw new RegraNegocioException(
                $"O CRM '{numero}' é inválido. Informe de 4 a 7 dígitos.");

        if (!Documentos.UfValida(uf ?? string.Empty))
            throw new RegraNegocioException(
                $"A UF '{uf}' é inválida. Use a sigla do estado, ex: SP.");

        return new Crm(
            Documentos.SomenteDigitos(numero!),
            uf!.Trim().ToUpperInvariant());
    }

    /// <summary>Reconstrói sem validar. Uso exclusivo do Entity Framework.</summary>
    public static Crm Reconstruir(string numero, string uf) => new(numero, uf);

    public override string ToString() => Completo;
}
