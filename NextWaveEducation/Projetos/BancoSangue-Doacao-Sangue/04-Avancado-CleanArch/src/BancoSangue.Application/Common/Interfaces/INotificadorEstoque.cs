namespace BancoSangue.Application.Common.Interfaces;

/// <summary>
/// Canal de notificação de alertas de estoque baixo.
/// A implementação pode logar, enviar e-mail, publicar em fila, etc.
/// </summary>
public interface INotificadorEstoque
{
    Task NotificarEstoqueBaixoAsync(
        string tipoSanguineo,
        int quantidadeAtualML,
        int quantidadeMinimaML,
        CancellationToken cancellationToken = default);
}
