namespace Clinica.Api.Util;

/// <summary>Apoio para lidar com os arquivos anexados ao atendimento.</summary>
public static class Arquivos
{
    /// <summary>
    /// Deduz o Content-Type pela extensão. Só é usado quando o cliente não
    /// manda o tipo: o navegador normalmente manda.
    /// </summary>
    public static string DetectarTipoConteudo(string nomeArquivo)
        => Path.GetExtension(nomeArquivo).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain; charset=utf-8",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument."
                       + "wordprocessingml.document",
            _ => "application/octet-stream"
        };
}
