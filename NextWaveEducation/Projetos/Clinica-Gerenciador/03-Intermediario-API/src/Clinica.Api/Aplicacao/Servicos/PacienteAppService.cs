using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Dominio.ObjetosDeValor;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;

namespace Clinica.Api.Aplicacao.Servicos;

/// <summary>
/// Orquestra o caso de uso de paciente: valida a entrada, consulta o
/// repositório para as regras que dependem do banco, chama o domínio e
/// devolve ViewModel.
/// </summary>
public class PacienteAppService
{
    private readonly IPacienteRepositorio _repositorio;
    private readonly IValidator<PacienteInputModel> _validador;

    public PacienteAppService(
        IPacienteRepositorio repositorio, IValidator<PacienteInputModel> validador)
    {
        _repositorio = repositorio;
        _validador = validador;
    }

    public async Task<List<PacienteViewModel>> PesquisarAsync(
        string? termo, CancellationToken cancellationToken = default)
    {
        var pacientes = await _repositorio.PesquisarAsync(termo, cancellationToken);
        return pacientes.Select(PacienteViewModel.De).ToList();
    }

    public async Task<FichaPacienteViewModel> ObterFichaAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var paciente = await _repositorio.ObterFichaAsync(id, cancellationToken)
                       ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

        return FichaPacienteViewModel.De(paciente);
    }

    public async Task<PacienteViewModel?> BuscarPorCpfAsync(
        string cpf, CancellationToken cancellationToken = default)
    {
        if (!Cpf.EhValido(cpf)) return null;

        var paciente = await _repositorio.ObterPorCpfAsync(Cpf.Criar(cpf), cancellationToken);
        return paciente is null ? null : PacienteViewModel.De(paciente);
    }

    public async Task<List<PacienteViewModel>> BuscarPorCelularAsync(
        string celular, CancellationToken cancellationToken = default)
    {
        var pacientes = await _repositorio.ListarPorCelularAsync(
            Celular.Criar(celular), cancellationToken);

        return pacientes.Select(PacienteViewModel.De).ToList();
    }

    public async Task<PacienteViewModel> CadastrarAsync(
        PacienteInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        var cpf = Cpf.Criar(entrada.Cpf);

        // Regra que depende do banco: o CPF identifica o paciente.
        if (await _repositorio.ExisteCpfAsync(cpf, null, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe um paciente cadastrado com o CPF {cpf.Formatado}.");

        var paciente = new Paciente(
            entrada.NomeCompleto,
            cpf,
            Celular.Criar(entrada.Celular),
            entrada.DataNascimento,
            entrada.Sexo,
            Email.CriarOpcional(entrada.Email),
            entrada.Observacoes);

        _repositorio.Adicionar(paciente);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PacienteViewModel.De(paciente);
    }

    public async Task<PacienteViewModel> AtualizarAsync(
        int id, PacienteInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        var paciente = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                       ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

        var cpf = Cpf.Criar(entrada.Cpf);

        if (await _repositorio.ExisteCpfAsync(cpf, id, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe outro paciente com o CPF {cpf.Formatado}.");

        paciente.AlterarDados(
            entrada.NomeCompleto,
            cpf,
            Celular.Criar(entrada.Celular),
            entrada.DataNascimento,
            entrada.Sexo,
            Email.CriarOpcional(entrada.Email),
            entrada.Observacoes);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return PacienteViewModel.De(paciente);
    }

    /// <summary>Remoção lógica: o histórico clínico é preservado.</summary>
    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var paciente = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                       ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(id, cancellationToken))
            throw new RegraNegocioException(
                $"'{paciente.NomeCompleto}' tem atendimento em aberto. " +
                "Cancele antes de remover o paciente.");

        paciente.Deletar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
