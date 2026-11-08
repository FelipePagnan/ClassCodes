using System.Text.RegularExpressions;
using Livros.Console.Entidades;
using Livros.Console.Excecoes;

namespace Livros.Console.Servicos;

/// <summary>
/// Gerenciamento de usuários: cadastro, atualização, remoção, listagem
/// e detalhes com as avaliações daquele usuário.
/// </summary>
public class UsuarioService
{
    private readonly List<Usuario> _usuarios = new();
    private int _proximoId = 1;

    public IReadOnlyList<Usuario> ListarTodos() => _usuarios.AsReadOnly();

    public Usuario? BuscarPorId(int id) => _usuarios.FirstOrDefault(u => u.Id == id);

    public Usuario ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Usuário com id {id} não encontrado.");

    public Usuario? BuscarPorEmail(string email)
        => _usuarios.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

    public Usuario Cadastrar(Usuario usuario)
    {
        ValidarDados(usuario);

        if (BuscarPorEmail(usuario.Email) is not null)
            throw new RegraNegocioException($"Já existe um usuário cadastrado com o e-mail '{usuario.Email}'.");

        usuario.Id = _proximoId++;
        usuario.Email = usuario.Email.Trim().ToLowerInvariant();
        usuario.Nome = usuario.Nome.Trim();
        usuario.DataCriacao = DateTime.Now;

        _usuarios.Add(usuario);
        return usuario;
    }

    public Usuario Atualizar(int id, Usuario dados)
    {
        var usuario = ObterPorId(id);
        ValidarDados(dados);

        var existente = BuscarPorEmail(dados.Email);
        if (existente is not null && existente.Id != id)
            throw new RegraNegocioException($"Já existe outro usuário com o e-mail '{dados.Email}'.");

        usuario.Nome = dados.Nome.Trim();
        usuario.Email = dados.Email.Trim().ToLowerInvariant();

        return usuario;
    }

    public void Remover(int id)
    {
        var usuario = ObterPorId(id);

        if (usuario.Avaliacoes.Count > 0)
            throw new RegraNegocioException(
                $"'{usuario.Nome}' possui {usuario.Avaliacoes.Count} avaliação(ões) e não pode ser removido.");

        _usuarios.Remove(usuario);
    }

    private static void ValidarDados(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nome))
            throw new RegraNegocioException("O nome é obrigatório.");

        if (usuario.Nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(usuario.Email))
            throw new RegraNegocioException("O e-mail é obrigatório.");

        if (!EmailValido(usuario.Email))
            throw new RegraNegocioException($"O e-mail '{usuario.Email}' é inválido.");
    }

    private static bool EmailValido(string email)
        => Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
}
