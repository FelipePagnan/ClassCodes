using Livros.Intermediario.Aplicacao.InputModels;
using Livros.Intermediario.Aplicacao.ViewModels;
using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Excecoes;
using Livros.Intermediario.Dominio.Interfaces;

namespace Livros.Intermediario.Aplicacao.Servicos;

public interface IUsuarioAppService
{
    Task<List<UsuarioViewModel>> ListarAsync();
    Task<UsuarioDetalheViewModel> ObterPorIdAsync(int id);
    Task<UsuarioViewModel> CadastrarAsync(CriarUsuarioInputModel input);
    Task<UsuarioViewModel> AtualizarAsync(int id, AtualizarUsuarioInputModel input);
    Task RemoverAsync(int id);
}

public class UsuarioAppService : IUsuarioAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuarioAppService> _logger;

    public UsuarioAppService(IUsuarioRepository usuarioRepository, ILogger<UsuarioAppService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<List<UsuarioViewModel>> ListarAsync()
    {
        var usuarios = await _usuarioRepository.ObterTodosAsync();
        return usuarios.Select(UsuarioViewModel.De).ToList();
    }

    /// <summary>Detalhes do usuário com as avaliações feitas por ele.</summary>
    public async Task<UsuarioDetalheViewModel> ObterPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Usuário", id);

        return UsuarioDetalheViewModel.DeDetalhe(usuario);
    }

    public async Task<UsuarioViewModel> CadastrarAsync(CriarUsuarioInputModel input)
    {
        if (await _usuarioRepository.ExisteEmailAsync(input.Email))
            throw new RegraNegocioException($"Já existe um usuário cadastrado com o e-mail '{input.Email}'.");

        var usuario = new Usuario(input.Nome, input.Email);

        await _usuarioRepository.AdicionarAsync(usuario);
        _logger.LogInformation("Usuário {UsuarioId} '{Nome}' cadastrado.", usuario.Id, usuario.Nome);

        return UsuarioViewModel.De(usuario);
    }

    public async Task<UsuarioViewModel> AtualizarAsync(int id, AtualizarUsuarioInputModel input)
    {
        var usuario = await _usuarioRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Usuário", id);

        if (await _usuarioRepository.ExisteEmailAsync(input.Email, ignorarId: id))
            throw new RegraNegocioException($"Já existe outro usuário com o e-mail '{input.Email}'.");

        usuario.Atualizar(input.Nome, input.Email);
        await _usuarioRepository.AtualizarAsync(usuario);

        return UsuarioViewModel.De(usuario);
    }

    public async Task RemoverAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdComAvaliacoesAsync(id)
            ?? throw NaoEncontradoException.Para("Usuário", id);

        usuario.GarantirQuePodeSerRemovido();

        await _usuarioRepository.RemoverAsync(usuario);
        _logger.LogInformation("Usuário {UsuarioId} removido.", id);
    }
}
