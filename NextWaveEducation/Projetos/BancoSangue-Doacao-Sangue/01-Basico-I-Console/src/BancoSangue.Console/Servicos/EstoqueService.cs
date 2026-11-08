using BancoSangue.Console.Entidades;
using BancoSangue.Console.Enums;

namespace BancoSangue.Console.Servicos;

/// <summary>
/// Controla o estoque de sangue por tipo sanguíneo + fator Rh.
/// </summary>
public class EstoqueService
{
    private readonly List<EstoqueSangue> _estoque = new();
    private int _proximoId = 1;

    public EstoqueService()
    {
        // Inicializa o estoque com todos os tipos sanguíneos possíveis, zerados.
        foreach (TipoSanguineo tipo in Enum.GetValues<TipoSanguineo>())
        {
            foreach (FatorRh fator in Enum.GetValues<FatorRh>())
            {
                _estoque.Add(new EstoqueSangue
                {
                    Id = _proximoId++,
                    TipoSanguineo = tipo,
                    FatorRh = fator,
                    QuantidadeML = 0
                });
            }
        }
    }

    public IReadOnlyList<EstoqueSangue> ListarTodos() => _estoque.AsReadOnly();

    public EstoqueSangue ObterPorTipo(TipoSanguineo tipo, FatorRh fator)
        => _estoque.First(e => e.TipoSanguineo == tipo && e.FatorRh == fator);

    /// <summary>
    /// Atualiza (soma) o estoque. Deve ser chamado sempre que uma doação for registrada.
    /// Retorna true se, após a atualização, o estoque ficou abaixo do mínimo (para emitir alerta - PLUS).
    /// </summary>
    public bool AtualizarEstoque(TipoSanguineo tipo, FatorRh fator, int quantidadeML)
    {
        var item = ObterPorTipo(tipo, fator);
        item.QuantidadeML += quantidadeML;
        return item.EstoqueBaixo;
    }

    public IEnumerable<EstoqueSangue> ListarTiposComEstoqueBaixo()
        => _estoque.Where(e => e.EstoqueBaixo);
}
