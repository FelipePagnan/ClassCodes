using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.ViewModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Infraestrutura.Repositorios;
using FluentValidation;

namespace Clinica.Api.Aplicacao.Servicos;

public class ServicoAppService
{
    private readonly IServicoRepositorio _repositorio;
    private readonly IValidator<ServicoInputModel> _validador;

    public ServicoAppService(
        IServicoRepositorio repositorio, IValidator<ServicoInputModel> validador)
    {
        _repositorio = repositorio;
        _validador = validador;
    }

    public async Task<List<ServicoViewModel>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var servicos = await _repositorio.ListarAsync(cancellationToken);
        return servicos.Select(ServicoViewModel.De).ToList();
    }

    public async Task<ServicoViewModel> ObterPorIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var servico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

        return ServicoViewModel.De(servico);
    }

    public async Task<ServicoViewModel> CadastrarAsync(
        ServicoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        if (await _repositorio.ExisteNomeAsync(entrada.Nome, null, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe um serviço chamado '{entrada.Nome.Trim()}'.");

        var servico = new Servico(
            entrada.Nome, entrada.Valor, entrada.DuracaoMinutos, entrada.Descricao);

        _repositorio.Adicionar(servico);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return ServicoViewModel.De(servico);
    }

    /// <summary>
    /// Atualiza o serviço. Valor e duração novos valem só para agendamentos
    /// futuros: cada atendimento guarda a própria cópia desses dois campos.
    /// </summary>
    public async Task<ServicoViewModel> AtualizarAsync(
        int id, ServicoInputModel entrada, CancellationToken cancellationToken = default)
    {
        await _validador.ValidateAndThrowAsync(entrada, cancellationToken);

        var servico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

        if (await _repositorio.ExisteNomeAsync(entrada.Nome, id, cancellationToken))
            throw new RegraNegocioException(
                $"Já existe outro serviço chamado '{entrada.Nome.Trim()}'.");

        servico.AlterarDados(
            entrada.Nome, entrada.Valor, entrada.DuracaoMinutos, entrada.Descricao);

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return ServicoViewModel.De(servico);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var servico = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

        if (await _repositorio.PossuiAtendimentoAbertoAsync(id, cancellationToken))
            throw new RegraNegocioException(
                $"O serviço '{servico.Nome}' tem atendimento em aberto.");

        servico.Deletar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
