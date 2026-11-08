using Clinica.Api.Enums;

namespace Clinica.Api.Entidades;

/// <summary>
/// Documento gerado num atendimento: atestado, receita ou evolução.
/// O conteúdo é armazenado como bytes.
/// </summary>
public class Anexo
{
    public const int TamanhoMaximoBytes = 10 * 1024 * 1024;

    public int Id { get; set; }

    public int AtendimentoId { get; set; }
    public Atendimento? Atendimento { get; set; }

    public TipoAnexo Tipo { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = "application/octet-stream";
    public byte[] Conteudo { get; set; } = Array.Empty<byte>();

    /// <summary>Texto livre do documento (o laudo em si, por exemplo).</summary>
    public string? Descricao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool EstaDeletado { get; set; }

    public int TamanhoBytes => Conteudo.Length;

    public string TamanhoLegivel => TamanhoBytes switch
    {
        < 1024 => $"{TamanhoBytes} B",
        < 1024 * 1024 => $"{TamanhoBytes / 1024.0:N1} KB",
        _ => $"{TamanhoBytes / 1024.0 / 1024.0:N1} MB"
    };

    public override string ToString()
        => $"[{Id}] {Tipo,-10} {NomeArquivo,-28} {TamanhoLegivel,10}  {DataCriacao:dd/MM/yyyy}";
}
