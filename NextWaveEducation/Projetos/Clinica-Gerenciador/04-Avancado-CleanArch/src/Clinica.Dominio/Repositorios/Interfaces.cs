using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.ObjetosDeValor;

namespace Clinica.Dominio.Repositorios;

/// <summary>
/// Os contratos de persistência moram no domínio, e a implementação com
/// Entity Framework mora na infraestrutura. É a inversão de dependência da
/// Clean Architecture: o domínio diz o que precisa e não sabe quem atende.
///
/// A diferença em relação ao nível Intermediário é que nenhum repositório
/// salva nada: salvar é trabalho do IUnitOfWork, para que uma operação que
/// mexe em várias entidades vire uma transação só.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Grava as alterações e, só depois de a gravação dar certo, publica os
    /// eventos de domínio acumulados pelas entidades.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa a operação dentro de uma transação do banco. Se já houver uma
    /// transação aberta, participa dela em vez de abrir outra.
    /// </summary>
    Task<T> ExecutarEmTransacaoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default);
}

public interface IPacienteRepositorio
{
    Task<Paciente?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Ficha completa: atendimentos, médicos, serviços e anexos.</summary>
    Task<Paciente?> ObterFichaAsync(int id, CancellationToken cancellationToken = default);

    Task<Paciente?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);

    /// <summary>Lista, porque o mesmo número atende mais de um paciente da família.</summary>
    Task<List<Paciente>> ListarPorCelularAsync(
        Celular celular, CancellationToken cancellationToken = default);

    Task<List<Paciente>> PesquisarAsync(
        string? termo, CancellationToken cancellationToken = default);

    Task<bool> ExisteCpfAsync(
        Cpf cpf, int? ignorarId = null, CancellationToken cancellationToken = default);

    Task<bool> PossuiAtendimentoAbertoAsync(
        int pacienteId, CancellationToken cancellationToken = default);

    void Adicionar(Paciente paciente);
}

public interface IMedicoRepositorio
{
    Task<List<Medico>> ListarAsync(
        string? especialidade = null, CancellationToken cancellationToken = default);

    Task<Medico?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Medico?> ObterPorCrmAsync(Crm crm, CancellationToken cancellationToken = default);

    Task<bool> ExisteCrmAsync(
        Crm crm, int? ignorarId = null, CancellationToken cancellationToken = default);

    Task<bool> PossuiAtendimentoAbertoAsync(
        int medicoId, CancellationToken cancellationToken = default);

    void Adicionar(Medico medico);
}

public interface IServicoRepositorio
{
    Task<List<Servico>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Servico?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNomeAsync(
        string nome, int? ignorarId = null, CancellationToken cancellationToken = default);

    Task<bool> PossuiAtendimentoAbertoAsync(
        int servicoId, CancellationToken cancellationToken = default);

    void Adicionar(Servico servico);
}

public interface IAtendimentoRepositorio
{
    Task<List<Atendimento>> ListarAsync(
        DateTime? dia = null,
        int? medicoId = null,
        int? pacienteId = null,
        StatusAtendimento? status = null,
        CancellationToken cancellationToken = default);

    Task<Atendimento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Atendimento?> ObterComAnexosAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atendimentos do mesmo dia que envolvem este médico ou este paciente.
    /// Alimenta a verificação de conflito de agenda.
    /// </summary>
    Task<List<Atendimento>> ListarDoDiaParaConflitoAsync(
        DateTime dia,
        int medicoId,
        int pacienteId,
        int? ignorarId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Atendimentos de amanhã, em aberto e sem lembrete enviado.</summary>
    Task<List<Atendimento>> ListarParaLembreteAsync(
        CancellationToken cancellationToken = default);

    Task<List<Anexo>> ListarAnexosDoPacienteAsync(
        int pacienteId, CancellationToken cancellationToken = default);

    void Adicionar(Atendimento atendimento);
}

public interface IUsuarioRepositorio
{
    Task<List<Usuario>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExisteUsuarioDoMedicoAsync(
        int medicoId, CancellationToken cancellationToken = default);

    Task<int> ContarAdministradoresAtivosAsync(CancellationToken cancellationToken = default);

    void Adicionar(Usuario usuario);
}
