using Clinica.Api.Data;
using Clinica.Api.Entidades;
using Clinica.Api.Enums;
using Clinica.Api.Excecoes;
using Clinica.Api.Modelos;
using Clinica.Api.Util;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Servicos;

/// <summary>
/// Cadastro de usuários e verificação de credenciais. Quem transforma isso
/// em identidade autenticada é o handler em Seguranca/.
/// </summary>
public class UsuarioService
{
    private readonly ClinicaDbContext _context;

    public UsuarioService(ClinicaDbContext context) => _context = context;

    public async Task<List<Usuario>> ListarAsync()
        => await _context.Usuarios
            .Include(u => u.Medico)
            .OrderBy(u => u.Nome)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Usuario> ObterPorIdAsync(int id)
        => await _context.Usuarios.Include(u => u.Medico).FirstOrDefaultAsync(u => u.Id == id)
           ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalizado = email.Trim().ToLowerInvariant();

        return await _context.Usuarios
            .Include(u => u.Medico)
            .FirstOrDefaultAsync(u => u.Email == normalizado);
    }

    /// <summary>
    /// Confere e-mail e senha. Devolve null quando qualquer um dos dois não
    /// bate, sem dizer qual — dizer "e-mail não existe" entrega ao atacante
    /// quais contas são válidas.
    /// </summary>
    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await BuscarPorEmailAsync(email);

        if (usuario is null || !usuario.Ativo) return null;
        if (!Senhas.Conferir(senha, usuario.SenhaHash, usuario.SenhaSalt)) return null;

        usuario.UltimoAcesso = DateTime.Now;
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario> CadastrarAsync(UsuarioRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Nome) || requisicao.Nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do usuário deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(requisicao.Email) || !requisicao.Email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{requisicao.Email}' é inválido.");

        if (!Enum.IsDefined(requisicao.Perfil))
            throw new RegraNegocioException($"Perfil '{requisicao.Perfil}' inválido.");

        if (!Senhas.SenhaForte(requisicao.Senha, out var motivo))
            throw new RegraNegocioException(motivo);

        var email = requisicao.Email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            throw new RegraNegocioException($"Já existe um usuário com o e-mail {email}.");

        // Perfil Médico precisa de vínculo: é o que permite ao médico ver
        // apenas a própria agenda.
        if (requisicao.Perfil == PerfilUsuario.Medico)
        {
            if (requisicao.MedicoId is null)
                throw new RegraNegocioException(
                    "Um usuário com perfil Médico precisa estar vinculado a um cadastro de médico.");

            var medico = await _context.Medicos
                             .FirstOrDefaultAsync(m =>
                                 m.Id == requisicao.MedicoId && !m.EstaDeletado)
                         ?? throw new RegraNegocioException(
                             $"Médico {requisicao.MedicoId} não encontrado.");

            if (await _context.Usuarios.AnyAsync(u => u.MedicoId == medico.Id))
                throw new RegraNegocioException(
                    $"O médico {medico.NomeCompleto} já possui usuário no sistema.");
        }
        else if (requisicao.MedicoId is not null)
        {
            throw new RegraNegocioException(
                "Só o perfil Médico aceita vínculo com cadastro de médico " +
                $"(perfil informado: {requisicao.Perfil}).");
        }

        var (hash, salt) = Senhas.Gerar(requisicao.Senha);

        var usuario = new Usuario
        {
            Nome = requisicao.Nome.Trim(),
            Email = email,
            SenhaHash = hash,
            SenhaSalt = salt,
            Perfil = requisicao.Perfil,
            MedicoId = requisicao.MedicoId,
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task AlterarSenhaAsync(int id, TrocaDeSenhaRequisicao requisicao)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        if (!Senhas.Conferir(requisicao.SenhaAtual, usuario.SenhaHash, usuario.SenhaSalt))
            throw new RegraNegocioException("A senha atual não confere.");

        if (!Senhas.SenhaForte(requisicao.SenhaNova, out var motivo))
            throw new RegraNegocioException(motivo);

        if (Senhas.Conferir(requisicao.SenhaNova, usuario.SenhaHash, usuario.SenhaSalt))
            throw new RegraNegocioException("A nova senha deve ser diferente da atual.");

        var (hash, salt) = Senhas.Gerar(requisicao.SenhaNova);
        usuario.SenhaHash = hash;
        usuario.SenhaSalt = salt;

        await _context.SaveChangesAsync();
    }

    public async Task DesativarAsync(int id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        if (usuario.Perfil == PerfilUsuario.Administrador
            && await _context.Usuarios.CountAsync(u =>
                u.Perfil == PerfilUsuario.Administrador && u.Ativo) == 1)
            throw new RegraNegocioException(
                "Não é possível desativar o último administrador ativo do sistema.");

        usuario.Ativo = false;
        await _context.SaveChangesAsync();
    }

    public async Task ReativarAsync(int id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        usuario.Ativo = true;
        await _context.SaveChangesAsync();
    }
}
