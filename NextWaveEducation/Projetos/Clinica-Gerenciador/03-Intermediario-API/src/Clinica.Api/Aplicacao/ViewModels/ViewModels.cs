using Clinica.Api.Dominio.Entidades;

namespace Clinica.Api.Aplicacao.ViewModels;

/// <summary>
/// O que sai pela API. Devolver ViewModel em vez da entidade resolve o laço
/// de serialização (paciente tem atendimentos, que têm paciente), esconde o
/// que não deve sair (hash e salt de senha, bytes de anexo) e deixa a
/// resposta estável mesmo quando a entidade muda.
///
/// Cada ViewModel traz um método De(...) que faz o mapeamento. Em projeto
/// grande isso costuma ir para AutoMapper ou Mapperly; aqui, à mão, fica
/// explícito o que é exposto.
/// </summary>
public record PacienteViewModel(
    int Id,
    string NomeCompleto,
    string Cpf,
    string Celular,
    string? Email,
    DateTime DataNascimento,
    int Idade,
    bool EhMenorDeIdade,
    string Sexo,
    string? Observacoes,
    DateTime DataCriacao)
{
    public static PacienteViewModel De(Paciente paciente) => new(
        paciente.Id,
        paciente.NomeCompleto,
        paciente.Cpf.Formatado,
        paciente.Celular.Formatado,
        paciente.Email?.Valor,
        paciente.DataNascimento,
        paciente.Idade,
        paciente.EhMenorDeIdade,
        paciente.Sexo.ToString(),
        paciente.Observacoes,
        paciente.DataCriacao);
}

/// <summary>Ficha do paciente: os dados mais o histórico de atendimentos.</summary>
public record FichaPacienteViewModel(
    PacienteViewModel Paciente,
    int TotalDeAtendimentos,
    List<AtendimentoResumoViewModel> Historico)
{
    public static FichaPacienteViewModel De(Paciente paciente)
    {
        var historico = paciente.Atendimentos
            .OrderByDescending(a => a.DataHora)
            .Select(AtendimentoResumoViewModel.De)
            .ToList();

        return new FichaPacienteViewModel(
            PacienteViewModel.De(paciente), historico.Count, historico);
    }
}

public record MedicoViewModel(
    int Id,
    string NomeCompleto,
    string Crm,
    string UfCrm,
    string CrmCompleto,
    string Especialidade,
    string? Email,
    string? Celular,
    DateTime DataCriacao)
{
    public static MedicoViewModel De(Medico medico) => new(
        medico.Id,
        medico.NomeCompleto,
        medico.Crm.Numero,
        medico.Crm.Uf,
        medico.Crm.Completo,
        medico.Especialidade,
        medico.Email?.Valor,
        medico.Celular?.Formatado,
        medico.DataCriacao);
}

public record ServicoViewModel(
    int Id,
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos)
{
    public static ServicoViewModel De(Servico servico) => new(
        servico.Id, servico.Nome, servico.Descricao, servico.Valor, servico.DuracaoMinutos);
}

/// <summary>Versão enxuta do atendimento, para listagens e agenda.</summary>
public record AtendimentoResumoViewModel(
    int Id,
    DateTime DataHora,
    DateTime DataHoraFim,
    int DuracaoMinutos,
    decimal Valor,
    string Status,
    string Paciente,
    string Medico,
    string Servico,
    bool ConfirmacaoEnviada,
    bool LembreteEnviado,
    int QuantidadeDeAnexos)
{
    public static AtendimentoResumoViewModel De(Atendimento atendimento) => new(
        atendimento.Id,
        atendimento.DataHora,
        atendimento.DataHoraFim,
        atendimento.DuracaoMinutos,
        atendimento.Valor,
        atendimento.Status.ToString(),
        atendimento.Paciente?.NomeCompleto ?? string.Empty,
        atendimento.Medico?.NomeCompleto ?? string.Empty,
        atendimento.Servico?.Nome ?? string.Empty,
        atendimento.ConfirmacaoEnviadaEm is not null,
        atendimento.LembreteEnviadoEm is not null,
        atendimento.Anexos.Count);
}

/// <summary>Atendimento completo, com os anexos listados.</summary>
public record AtendimentoViewModel(
    int Id,
    DateTime DataHora,
    DateTime DataHoraFim,
    int DuracaoMinutos,
    decimal Valor,
    string Status,
    string? Observacoes,
    DateTime? ConfirmacaoEnviadaEm,
    DateTime? LembreteEnviadoEm,
    PacienteViewModel Paciente,
    MedicoViewModel Medico,
    ServicoViewModel Servico,
    List<AnexoViewModel> Anexos)
{
    public static AtendimentoViewModel De(Atendimento atendimento) => new(
        atendimento.Id,
        atendimento.DataHora,
        atendimento.DataHoraFim,
        atendimento.DuracaoMinutos,
        atendimento.Valor,
        atendimento.Status.ToString(),
        atendimento.Observacoes,
        atendimento.ConfirmacaoEnviadaEm,
        atendimento.LembreteEnviadoEm,
        PacienteViewModel.De(atendimento.Paciente),
        MedicoViewModel.De(atendimento.Medico),
        ServicoViewModel.De(atendimento.Servico),
        atendimento.Anexos.Select(AnexoViewModel.De).ToList());
}

/// <summary>
/// Anexo sem o conteúdo: os bytes saem só pelo endpoint de download, para a
/// listagem não arrastar megabytes.
/// </summary>
public record AnexoViewModel(
    int Id,
    int AtendimentoId,
    string Tipo,
    string NomeArquivo,
    string TipoConteudo,
    int TamanhoBytes,
    string TamanhoLegivel,
    string? Descricao,
    DateTime DataCriacao)
{
    public static AnexoViewModel De(Anexo anexo) => new(
        anexo.Id,
        anexo.AtendimentoId,
        anexo.Tipo.ToString(),
        anexo.NomeArquivo,
        anexo.TipoConteudo,
        anexo.TamanhoBytes,
        anexo.TamanhoLegivel,
        anexo.Descricao,
        anexo.DataCriacao);
}

public record UsuarioViewModel(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    int? MedicoId,
    string? Medico,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? UltimoAcesso)
{
    public static UsuarioViewModel De(Usuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.Email.Valor,
        usuario.Perfil.ToString(),
        usuario.MedicoId,
        usuario.Medico?.NomeCompleto,
        usuario.Ativo,
        usuario.DataCriacao,
        usuario.UltimoAcesso);
}

/// <summary>Resposta do login: o token e o que o cliente precisa saber dele.</summary>
public record LoginViewModel(
    string Token,
    DateTime ExpiraEm,
    UsuarioViewModel Usuario);

/// <summary>Agenda de um dia, com os totais por status.</summary>
public record AgendaDoDiaViewModel(
    DateTime Dia,
    int Total,
    int Agendados,
    int Confirmados,
    int Realizados,
    int Cancelados,
    int Faltas,
    decimal ValorPrevisto,
    List<AtendimentoResumoViewModel> Atendimentos);
