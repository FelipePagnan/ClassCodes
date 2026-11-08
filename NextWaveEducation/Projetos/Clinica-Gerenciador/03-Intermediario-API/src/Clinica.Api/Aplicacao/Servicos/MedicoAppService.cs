using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Dominio.ObjetosDeValor;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;

namespace Clinica.Api.Aplicacao.Servicos;

public class MedicoAppService
{
    private readonly IMedicoRepositorio _repositorio;
    private readonly IValidator<MedicoInputModel> _validador;

    public MedicoAppService(
        IMedicoRepositorio repositorio, IValidator<MedicoInputModel> validador)
    {
        _repositorio = repositorio;
        _validador = validador;
    }

    public async Task<List<MedicoViewModel>> ListarAsync(
        string? especialidade, CancellationToken cancellationToken = default)
    {
        var medicos = await _repositorio.ListarAsync(especialidade, cancellationToken);
        return medicos.Select(MedicoViewModel.De).ToList();
    }

    public async Task<MedicoViewModel> ObterPorIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var medico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                     ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

        return MedicoViewModel.De(medico);
    }

    /// <summary>Busca pelo par CRM + UF, que é o que identifica o médico.</summary>
    public async Task<MedicoViewModel?> BuscarPorCrmAsync(
        string crm, string uf, CancellationToken cancellationToken = default)
    {
        var medico = await _repositorio.ObterPorCrmAsync(Crm.Criar(crm, uf), cancellationToken);
        return medico is null ? null : MedicoViewModel.De(medico);
    }

    public async Task<MedicoViewModel> CadastrarAsync(
        MedicoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        var crm = Crm.Criar(entrada.Crm, entrada.UfCrm);

        if (await _repositorio.ExisteCrmAsync(crm, null, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe um médico cadastrado com {crm.Completo}.");

        var medico = new Medico(
            entrada.NomeCompleto,
            crm,
            entrada.Especialidade,
            Email.CriarOpcional(entrada.Email),
            Celular.CriarOpcional(entrada.Celular));

        _repositorio.Adicionar(medico);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return MedicoViewModel.De(medico);
    }

    public async Task<MedicoViewModel> AtualizarAsync(
        int id, MedicoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        var medico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                     ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

        var crm = Crm.Criar(entrada.Crm, entrada.UfCrm);

        if (await _repositorio.ExisteCrmAsync(crm, id, cancellationToken))
            throw new RegraNegocioException($"Já existe outro médico com {crm.Completo}.");

        medico.AlterarDados(
            entrada.NomeCompleto,
            crm,
            entrada.Especialidade,
            Email.CriarOpcional(entrada.Email),
            Celular.CriarOpcional(entrada.Celular));

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return MedicoViewModel.De(medico);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var medico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                     ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(id, cancellationToken))
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} tem atendimento em aberto. " +
                "Cancele ou remarque antes de remover o médico.");

        medico.Deletar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
