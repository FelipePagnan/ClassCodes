using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Enums;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Dominio.ObjetosDeValor;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Api.Aplicacao.Servicos;

/// <summary>
/// Login, emissão do token JWT e cadastro de usuários nos três perfis.
/// </summary>
public class UsuarioAppService
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IMedicoRepositorio _medicoRepositorio;
    private readonly IValidator<UsuarioInputModel> _validadorUsuario;
    private readonly IValidator<LoginInputModel> _validadorLogin;
    private readonly IValidator<TrocaDeSenhaInputModel> _validadorSenha;
    private readonly JwtConfiguracao _jwt;

    public UsuarioAppService(
        IUsuarioRepositorio repositorio,
        IMedicoRepositorio medicoRepositorio,
        IValidator<UsuarioInputModel> validadorUsuario,
        IValidator<LoginInputModel> validadorLogin,
        IValidator<TrocaDeSenhaInputModel> validadorSenha,
        IOptions<JwtConfiguracao> jwt)
    {
        _repositorio = repositorio;
        _medicoRepositorio = medicoRepositorio;
        _validadorUsuario = validadorUsuario;
        _validadorLogin = validadorLogin;
        _validadorSenha = validadorSenha;
        _jwt = jwt.Value;
    }

    // --------------------------------- Login ---------------------------------

    public async Task<LoginViewModel> LoginAsync(
        LoginInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorLogin.ValidateAndThrowAsync(entrada, cancellationToken);

        // Mensagem genérica de propósito: dizer "e-mail não existe" entrega
        // ao atacante quais contas são válidas.
        const string erro = "E-mail ou senha inválidos.";

        if (!Email.EhValido(entrada.Email))
            throw new RegraNegocioException(erro);

        var usuario = await _repositorio.ObterPorEmailAsync(
            Email.Criar(entrada.Email), cancellationToken);

        if (usuario is null || !usuario.ConferirSenha(entrada.Senha))
            throw new RegraNegocioException(erro);

        if (!usuario.Ativo)
            throw new RegraNegocioException(
                "Este usuário está inativo. Procure o administrador.");

        usuario.RegistrarAcesso();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        var (token, expiraEm) = GerarToken(usuario);

        return new LoginViewModel(token, expiraEm, UsuarioViewModel.De(usuario));
    }

    private (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwt.MinutosDeValidade);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email.Valor),
            new(ClaimTypes.Role, usuario.Perfil.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // O vínculo com o médico vai no token: é assim que a API filtra a
        // agenda sem precisar consultar o banco a cada requisição.
        if (usuario.MedicoId is not null)
            claims.Add(new Claim(ClaimsDaClinica.MedicoId, usuario.MedicoId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _jwt.Emissor,
            audience: _jwt.Audiencia,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(
                _jwt.ObterChave(), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }

    // -------------------------------- Cadastro --------------------------------

    public async Task<List<UsuarioViewModel>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _repositorio.ListarAsync(cancellationToken);
        return usuarios.Select(UsuarioViewModel.De).ToList();
    }

    public async Task<UsuarioViewModel> ObterPorIdAsync(
        int id, CancellationToken cancellationToken = default)
        => UsuarioViewModel.De(
            await _repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException($"Usuário {id} não encontrado."));

    public async Task<UsuarioViewModel> CadastrarAsync(
        UsuarioInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorUsuario.ValidateAndThrowAsync(entrada, cancellationToken);

        var email = Email.Criar(entrada.Email);

        if (await _repositorio.ExisteEmailAsync(email, cancellationToken))
            throw new RegraNegocioException($"Já existe um usuário com o e-mail {email.Valor}.");

        Medico? medico = null;

        if (entrada.Perfil == PerfilUsuario.Medico)
        {
            medico = await _medicoRepositorio.ObterPorIdAsync(
                         entrada.MedicoId!.Value, cancellationToken)
                     ?? throw new RegraNegocioException(
                         $"Médico {entrada.MedicoId} não encontrado.");

            if (await _repositorio.ExisteUsuarioDoMedicoAsync(medico.Id, cancellationToken))
                throw new RegraNegocioException(
                    $"O médico {medico.NomeCompleto} já possui usuário no sistema.");
        }

        var usuario = new Usuario(
            entrada.Nome, email, entrada.Senha, entrada.Perfil, medico);

        _repositorio.Adicionar(usuario);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return UsuarioViewModel.De(usuario);
    }

    public async Task AlterarSenhaAsync(
        int id, TrocaDeSenhaInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validadorSenha.ValidateAndThrowAsync(entrada, cancellationToken);

        var usuario = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        usuario.AlterarSenha(entrada.SenhaAtual, entrada.SenhaNova);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        // Desativar o último administrador deixaria o sistema sem ninguém
        // capaz de reativá-lo.
        if (usuario.Perfil == PerfilUsuario.Administrador
            && await _repositorio.ContarAdministradoresAtivosAsync(cancellationToken) == 1)
            throw new RegraNegocioException(
                "Não é possível desativar o último administrador ativo do sistema.");

        usuario.Desativar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task ReativarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Usuário {id} não encontrado.");

        usuario.Reativar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
