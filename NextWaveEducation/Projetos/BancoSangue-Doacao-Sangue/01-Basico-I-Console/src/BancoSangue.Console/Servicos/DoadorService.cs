using System.Text.RegularExpressions;
using BancoSangue.Console.Entidades;
using BancoSangue.Console.Excecoes;

namespace BancoSangue.Console.Servicos;

/// <summary>
/// Responsável pelo cadastro e consulta de doadores, incluindo validação de dados.
/// </summary>
public class DoadorService
{
    private readonly List<Doador> _doadores = new();
    private int _proximoId = 1;
    private int _proximoIdEndereco = 1;

    public IReadOnlyList<Doador> ListarTodos() => _doadores.AsReadOnly();

    public Doador Cadastrar(Doador doador)
    {
        ValidarDados(doador);

        doador.Id = _proximoId++;
        if (doador.Endereco is not null)
        {
            doador.Endereco.Id = _proximoIdEndereco++;
            doador.Endereco.DoadorId = doador.Id;
        }

        _doadores.Add(doador);
        return doador;
    }

    public Doador? BuscarPorId(int id) => _doadores.FirstOrDefault(d => d.Id == id);

    public Doador? BuscarPorEmail(string email) =>
        _doadores.FirstOrDefault(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Doacao> ConsultarHistoricoDoacoes(int doadorId)
    {
        var doador = BuscarPorId(doadorId);
        if (doador is null)
            throw new RegraNegocioException($"Doador com id {doadorId} não encontrado.");

        return doador.Doacoes.OrderByDescending(d => d.DataDoacao);
    }

    private void ValidarDados(Doador doador)
    {
        if (string.IsNullOrWhiteSpace(doador.NomeCompleto))
            throw new RegraNegocioException("O nome completo é obrigatório.");

        if (string.IsNullOrWhiteSpace(doador.Email) || !EmailValido(doador.Email))
            throw new RegraNegocioException("O e-mail informado é inválido.");

        // Regra de negócio: não permitir cadastrar doador com o mesmo e-mail.
        if (BuscarPorEmail(doador.Email) is not null)
            throw new RegraNegocioException($"Já existe um doador cadastrado com o e-mail '{doador.Email}'.");

        if (doador.DataNascimento == default || doador.DataNascimento > DateTime.Today)
            throw new RegraNegocioException("Data de nascimento inválida.");

        if (doador.Peso <= 0)
            throw new RegraNegocioException("O peso deve ser maior que zero.");

        // Observação: menor de idade PODE ter cadastro. A restrição de doar é aplicada em DoacaoService.
    }

    private static bool EmailValido(string email)
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
