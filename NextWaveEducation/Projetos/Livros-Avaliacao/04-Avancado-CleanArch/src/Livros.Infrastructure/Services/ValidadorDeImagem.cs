using Livros.Application.Common.Interfaces;

namespace Livros.Infrastructure.Services;

/// <summary>
/// Valida imagens pela assinatura binária (magic number), não apenas pela
/// extensão do arquivo, evitando que um .exe renomeado para .jpg passe.
/// </summary>
public class ValidadorDeImagem : IValidadorDeImagem
{
    private static readonly string[] _extensoes = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    public IReadOnlyList<string> ExtensoesPermitidas => _extensoes;

    public bool ExtensaoPermitida(string nomeArquivo)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo)) return false;

        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        return _extensoes.Contains(extensao);
    }

    public bool EhImagemValida(byte[] bytes)
    {
        if (bytes is null || bytes.Length < 12) return false;

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;

        // GIF: "GIF8"
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38) return true;

        // BMP: "BM"
        if (bytes[0] == 0x42 && bytes[1] == 0x4D) return true;

        // WEBP: "RIFF" .... "WEBP"
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50) return true;

        return false;
    }

    public string DetectarTipoConteudo(byte[] bytes)
    {
        if (bytes is null || bytes.Length < 4) return "application/octet-stream";

        if (bytes[0] == 0xFF && bytes[1] == 0xD8) return "image/jpeg";
        if (bytes[0] == 0x89 && bytes[1] == 0x50) return "image/png";
        if (bytes[0] == 0x47 && bytes[1] == 0x49) return "image/gif";
        if (bytes[0] == 0x42 && bytes[1] == 0x4D) return "image/bmp";
        if (bytes[0] == 0x52 && bytes[1] == 0x49) return "image/webp";

        return "application/octet-stream";
    }
}
