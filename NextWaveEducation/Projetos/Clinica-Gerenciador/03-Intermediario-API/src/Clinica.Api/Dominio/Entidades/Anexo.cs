using Clinica.Api.Dominio.Enums;
using Clinica.Api.Dominio.Excecoes;

namespace Clinica.Api.Dominio.Entidades;

/// <summary>
/// Documento gerado num atendimento: atestado, receita ou evolução. O
/// conteúdo fica como bytes na própria tabela — não é o ideal num cenário
/// real (o lugar disso é um storage), mas atende ao treinamento.
/// </summary>
public class Anexo : EntidadeBase
{
    public const int TamanhoMaximoBytes = 10 * 1024 * 1024;

    private Anexo() { }

    internal Anexo(
        Atendimento atendimento,
        TipoAnexo tipo,
        string nomeArquivo,
        string tipoConteudo,
        byte[] conteudo,
        string? descricao)
    {
        if (!Enum.IsDefined(tipo))
            throw new RegraNegocioException($"Tipo de anexo '{tipo}' inválido.");

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new RegraNegocioException("O nome do arquivo é obrigatório.");

        if (nomeArquivo.Trim().Length > 255)
            throw new RegraNegocioException("O nome do arquivo deve ter no máximo 255 caracteres.");

        if (conteudo is null || conteudo.Length == 0)
            throw new RegraNegocioException("O arquivo do anexo está vazio.");

        if (conteudo.Length > TamanhoMaximoBytes)
            throw new RegraNegocioException(
                $"O anexo deve ter no máximo {TamanhoMaximoBytes / 1024 / 1024}MB.");

        AtendimentoId = atendimento.Id;
        Atendimento = atendimento;
        Tipo = tipo;
        NomeArquivo = nomeArquivo.Trim();
        TipoConteudo = string.IsNullOrWhiteSpace(tipoConteudo)
            ? "application/octet-stream"
            : tipoConteudo;
        Conteudo = conteudo;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        DataCriacao = DateTime.Now;
    }

    public int AtendimentoId { get; private set; }
    public Atendimento Atendimento { get; private set; } = null!;

    public TipoAnexo Tipo { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string TipoConteudo { get; private set; } = "application/octet-stream";
    public byte[] Conteudo { get; private set; } = Array.Empty<byte>();

    /// <summary>Texto livre do documento (o laudo em si, por exemplo).</summary>
    public string? Descricao { get; private set; }

    public int TamanhoBytes => Conteudo.Length;

    public string TamanhoLegivel => TamanhoBytes switch
    {
        < 1024 => $"{TamanhoBytes} B",
        < 1024 * 1024 => $"{TamanhoBytes / 1024.0:N1} KB",
        _ => $"{TamanhoBytes / 1024.0 / 1024.0:N1} MB"
    };
}
