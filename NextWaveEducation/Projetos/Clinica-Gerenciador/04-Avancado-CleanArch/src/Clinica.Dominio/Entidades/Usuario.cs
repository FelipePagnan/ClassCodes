using Clinica.Dominio.Enums;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.ObjetosDeValor;
using Clinica.Dominio.Servicos;

namespace Clinica.Dominio.Entidades;

/// <summary>
/// Usuário do sistema. A senha nunca é guardada em texto: só o hash PBKDF2
/// e o salt que o gerou.
/// </summary>
public class Usuario : EntidadeBase
{
    private Usuario() { }

    public Usuario(
        string nome,
        Email email,
        string senha,
        PerfilUsuario perfil,
        Medico? medico = null)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do usuário deve ter ao menos 3 caracteres.");

        if (!Enum.IsDefined(perfil))
            throw new RegraNegocioException($"Perfil '{perfil}' inválido.");

        // Perfil Médico precisa de vínculo: é o que permite ao médico ver
        // apenas a própria agenda.
        if (perfil == PerfilUsuario.Medico && medico is null)
            throw new RegraNegocioException(
                "Um usuário com perfil Médico precisa estar vinculado a um cadastro de médico.");

        if (perfil != PerfilUsuario.Medico && medico is not null)
            throw new RegraNegocioException(
                "Só o perfil Médico aceita vínculo com cadastro de médico " +
                $"(perfil informado: {perfil}).");

        Nome = nome.Trim();
        Email = email;
        Perfil = perfil;
        MedicoId = medico?.Id;
        Medico = medico;
        Ativo = true;
        DataCriacao = DateTime.Now;

        DefinirSenha(senha);
    }

    public string Nome { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = string.Empty;
    public string SenhaSalt { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; }

    public int? MedicoId { get; private set; }
    public Medico? Medico { get; private set; }

    public bool Ativo { get; private set; } = true;
    public DateTime? UltimoAcesso { get; private set; }

    public bool ConferirSenha(string senha) => Senhas.Conferir(senha, SenhaHash, SenhaSalt);

    public void AlterarSenha(string senhaAtual, string senhaNova)
    {
        if (!ConferirSenha(senhaAtual))
            throw new RegraNegocioException("A senha atual não confere.");

        if (ConferirSenha(senhaNova))
            throw new RegraNegocioException("A nova senha deve ser diferente da atual.");

        DefinirSenha(senhaNova);
    }

    public void RegistrarAcesso() => UltimoAcesso = DateTime.Now;

    public void Desativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    private void DefinirSenha(string senha)
    {
        if (!Senhas.SenhaForte(senha, out var motivo))
            throw new RegraNegocioException(motivo);

        var (hash, salt) = Senhas.Gerar(senha);
        SenhaHash = hash;
        SenhaSalt = salt;
    }
}
