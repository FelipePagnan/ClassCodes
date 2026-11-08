namespace Livros.Application.Common.Interfaces;

/// <summary>
/// Valida se um arquivo enviado é realmente uma imagem, conferindo a
/// assinatura binária (magic number) e não apenas a extensão.
/// </summary>
public interface IValidadorDeImagem
{
    bool EhImagemValida(byte[] bytes);
    bool ExtensaoPermitida(string nomeArquivo);
    string DetectarTipoConteudo(byte[] bytes);
    IReadOnlyList<string> ExtensoesPermitidas { get; }
}
