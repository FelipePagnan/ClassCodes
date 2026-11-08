using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Enums;
using Clinica.Api.Dominio.ObjetosDeValor;

namespace Clinica.Api.Infraestrutura.Repositorios;

/// <summary>
/// Contratos de acesso a dados. O serviço de aplicação depende destas
/// interfaces e não do Entity Framework, o que deixa a regra de negócio
/// testável sem banco.
///
/// Cada repositório ainda salva as próprias alterações. No nível Avançado
/// isso sai daqui e vira Unit of Work, para que uma operação que mexe em
/// duas entidades vire uma transação só.
/// </summary>
public interface IRepositorio
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

public interface IPacienteRepositorio : IRepositorio
{
    Task<List<Paciente>> ListarAsync(CancellationToken cancellationToken = default);

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

public interface IMedicoRepositorio : IRepositorio
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

public interface IServicoRepositorio : IRepositorio
{
    Task<List<Servico>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Servico?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNomeAsync(
        string nome, int? ignorarId = null, CancellationToken cancellationToken = default);

    Task<bool> PossuiAtendimentoAbertoAsync(
        int servicoId, CancellationToken cancellationToken = default);

    void Adicionar(Servico servico);
}

public interface IAtendimentoRepositorio : IRepositorio
{
    Task<List<Atendimento>> ListarAsync(
        DateTime? dia = null,
        int? medicoId = null,
        int? pacienteId = null,
        StatusAtendimento? status = null,
        CancellationToken cancellationToken = default);

    Task<Atendimento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Com os anexos carregados, para a tela de detalhe.</summary>
    Task<Atendimento?> ObterComAnexosAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atendimentos do mesmo dia que envolvem este médico ou este paciente.
    /// É a consulta que alimenta a verificação de conflito.
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

public interface IUsuarioRepositorio : IRepositorio
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
