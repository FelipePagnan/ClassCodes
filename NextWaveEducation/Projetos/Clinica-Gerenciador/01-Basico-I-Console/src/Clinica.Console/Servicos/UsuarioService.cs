using Clinica.Console.Entidades;
using Clinica.Console.Enums;
using Clinica.Console.Excecoes;
using Clinica.Console.Util;

namespace Clinica.Console.Servicos;

/// <summary>
/// Autenticação e autorização com três perfis: Administrador, Médico e
/// Recepcionista. No Console a "sessão" é só o usuário logado em memória;
/// nos níveis seguintes isso vira JWT, mas as regras de permissão são as
/// mesmas, por isso ficam centralizadas aqui.
/// </summary>
public class UsuarioService
{
    private readonly MedicoService _medicoService;

    private readonly List<Usuario> _usuarios = new();
    private int _proximoId = 1;

    public UsuarioService(MedicoService medicoService) => _medicoService = medicoService;

    /// <summary>Usuário autenticado na sessão atual, ou null se ninguém logou.</summary>
    public Usuario? UsuarioLogado { get; private set; }

    public bool EstaAutenticado => UsuarioLogado is not null;

    public IReadOnlyList<Usuario> ListarTodos() => _usuarios.AsReadOnly();

    public Usuario? BuscarPorId(int id) => _usuarios.FirstOrDefault(u => u.Id == id);

    public Usuario? BuscarPorEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalizado = email.Trim().ToLowerInvariant();
        return _usuarios.FirstOrDefault(u => u.Email == normalizado);
    }

    public Usuario ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

    // ------------------------------- Cadastro -------------------------------

    public Usuario Cadastrar(
        string nome, string email, string senha, PerfilUsuario perfil, int? medicoId = null)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do usuário deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{email}' é inválido.");

        if (!Enum.IsDefined(perfil))
            throw new RegraNegocioException($"Perfil '{perfil}' inválido.");

        if (!Senhas.SenhaForte(senha, out var motivo))
            throw new RegraNegocioException(motivo);

        var normalizado = email.Trim().ToLowerInvariant();

        if (BuscarPorEmail(normalizado) is not null)
            throw new RegraNegocioException($"Já existe um usuário com o e-mail {normalizado}.");

        // Perfil Médico precisa estar vinculado a um cadastro de médico,
        // senão não há agenda para o sistema filtrar.
        if (perfil == PerfilUsuario.Medico)
        {
            if (medicoId is null)
                throw new RegraNegocioException(
                    "Um usuário com perfil Médico precisa estar vinculado a um cadastro de médico.");

            var medico = _medicoService.ObterPorId(medicoId.Value);

            if (_usuarios.Any(u => u.MedicoId == medico.Id))
                throw new RegraNegocioException(
                    $"O médico {medico.NomeCompleto} já possui usuário no sistema.");
        }
        else if (medicoId is not null)
        {
            throw new RegraNegocioException(
                $"Só o perfil Médico aceita vínculo com cadastro de médico (perfil informado: {perfil}).");
        }

        var (hash, salt) = Senhas.Gerar(senha);

        var usuario = new Usuario
        {
            Id = _proximoId++,
            Nome = nome.Trim(),
            Email = normalizado,
            SenhaHash = hash,
            SenhaSalt = salt,
            Perfil = perfil,
            MedicoId = medicoId,
            Medico = medicoId is null ? null : _medicoService.BuscarPorId(medicoId.Value),
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        _usuarios.Add(usuario);
        return usuario;
    }

    public Usuario AlterarSenha(int id, string senhaAtual, string senhaNova)
    {
        var usuario = ObterPorId(id);

        if (!Senhas.Conferir(senhaAtual, usuario.SenhaHash, usuario.SenhaSalt))
            throw new RegraNegocioException("A senha atual não confere.");

        if (!Senhas.SenhaForte(senhaNova, out var motivo))
            throw new RegraNegocioException(motivo);

        if (Senhas.Conferir(senhaNova, usuario.SenhaHash, usuario.SenhaSalt))
            throw new RegraNegocioException("A nova senha deve ser diferente da atual.");

        var (hash, salt) = Senhas.Gerar(senhaNova);
        usuario.SenhaHash = hash;
        usuario.SenhaSalt = salt;

        return usuario;
    }

    /// <summary>Desativa o usuário sem apagar o registro (histórico de acesso).</summary>
    public void Desativar(int id)
    {
        var usuario = ObterPorId(id);

        if (usuario.Perfil == PerfilUsuario.Administrador
            && _usuarios.Count(u => u.Perfil == PerfilUsuario.Administrador && u.Ativo) == 1)
            throw new RegraNegocioException(
                "Não é possível desativar o último administrador ativo do sistema.");

        usuario.Ativo = false;

        if (UsuarioLogado?.Id == id)
            Logout();
    }

    public void Reativar(int id) => ObterPorId(id).Ativo = true;

    // ------------------------------ Autenticação ------------------------------

    public Usuario Login(string email, string senha)
    {
        var usuario = BuscarPorEmail(email);

        // Mensagem genérica de propósito: dizer "e-mail não existe" entrega
        // ao atacante quais contas são válidas.
        if (usuario is null || !Senhas.Conferir(senha, usuario.SenhaHash, usuario.SenhaSalt))
            throw new RegraNegocioException("E-mail ou senha inválidos.");

        if (!usuario.Ativo)
            throw new RegraNegocioException("Este usuário está inativo. Procure o administrador.");

        usuario.UltimoAcesso = DateTime.Now;
        UsuarioLogado = usuario;

        return usuario;
    }

    public void Logout() => UsuarioLogado = null;

    // ------------------------------ Autorização ------------------------------

    /// <summary>Exige que alguém esteja logado e com um dos perfis informados.</summary>
    public Usuario Exigir(params PerfilUsuario[] perfis)
    {
        var usuario = UsuarioLogado
            ?? throw new RegraNegocioException("É necessário fazer login para esta operação.");

        if (perfis.Length > 0 && !perfis.Contains(usuario.Perfil))
            throw new RegraNegocioException(
                $"O perfil {usuario.Perfil} não tem permissão para esta operação. " +
                $"Perfis permitidos: {string.Join(", ", perfis)}.");

        return usuario;
    }

    public bool Pode(params PerfilUsuario[] perfis)
        => UsuarioLogado is not null
           && (perfis.Length == 0 || perfis.Contains(UsuarioLogado.Perfil));

    /// <summary>
    /// Filtra a agenda pelo usuário logado: médico vê só os atendimentos dele,
    /// administrador e recepcionista veem todos.
    /// </summary>
    public IEnumerable<Atendimento> FiltrarPorPermissao(IEnumerable<Atendimento> atendimentos)
    {
        var usuario = UsuarioLogado;

        if (usuario is null) return Enumerable.Empty<Atendimento>();

        if (usuario.Perfil == PerfilUsuario.Medico && usuario.MedicoId is not null)
            return atendimentos.Where(a => a.MedicoId == usuario.MedicoId);

        return atendimentos;
    }
}
