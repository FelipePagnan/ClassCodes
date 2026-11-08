using Clinica.Dominio.Entidades;

namespace Clinica.Aplicacao.Dtos;

/// <summary>
/// O que sai pela API. Devolver Dto em vez da entidade resolve o laço
/// de serialização (paciente tem atendimentos, que têm paciente), esconde o
/// que não deve sair (hash e salt de senha, bytes de anexo) e deixa a
/// resposta estável mesmo quando a entidade muda.
///
/// Cada Dto traz um método De(...) que faz o mapeamento. Em projeto
/// grande isso costuma ir para AutoMapper ou Mapperly; aqui, à mão, fica
/// explícito o que é exposto.
/// </summary>
public record PacienteDto(
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
    public static PacienteDto De(Paciente paciente) => new(
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
public record FichaPacienteDto(
    PacienteDto Paciente,
    int TotalDeAtendimentos,
    List<AtendimentoResumoDto> Historico)
{
    public static FichaPacienteDto De(Paciente paciente)
    {
        var historico = paciente.Atendimentos
            .OrderByDescending(a => a.DataHora)
            .Select(AtendimentoResumoDto.De)
            .ToList();

        return new FichaPacienteDto(
            PacienteDto.De(paciente), historico.Count, historico);
    }
}

public record MedicoDto(
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
    public static MedicoDto De(Medico medico) => new(
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

public record ServicoDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos)
{
    public static ServicoDto De(Servico servico) => new(
        servico.Id, servico.Nome, servico.Descricao, servico.Valor, servico.DuracaoMinutos);
}

/// <summary>Versão enxuta do atendimento, para listagens e agenda.</summary>
public record AtendimentoResumoDto(
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
    public static AtendimentoResumoDto De(Atendimento atendimento) => new(
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
public record AtendimentoDto(
    int Id,
    DateTime DataHora,
    DateTime DataHoraFim,
    int DuracaoMinutos,
    decimal Valor,
    string Status,
    string? Observacoes,
    DateTime? ConfirmacaoEnviadaEm,
    DateTime? LembreteEnviadoEm,
    PacienteDto Paciente,
    MedicoDto Medico,
    ServicoDto Servico,
    List<AnexoDto> Anexos)
{
    public static AtendimentoDto De(Atendimento atendimento) => new(
        atendimento.Id,
        atendimento.DataHora,
        atendimento.DataHoraFim,
        atendimento.DuracaoMinutos,
        atendimento.Valor,
        atendimento.Status.ToString(),
        atendimento.Observacoes,
        atendimento.ConfirmacaoEnviadaEm,
        atendimento.LembreteEnviadoEm,
        PacienteDto.De(atendimento.Paciente),
        MedicoDto.De(atendimento.Medico),
        ServicoDto.De(atendimento.Servico),
        atendimento.Anexos.Select(AnexoDto.De).ToList());
}

/// <summary>
/// Anexo sem o conteúdo: os bytes saem só pelo endpoint de download, para a
/// listagem não arrastar megabytes.
/// </summary>
public record AnexoDto(
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
    public static AnexoDto De(Anexo anexo) => new(
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

public record UsuarioDto(
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
    public static UsuarioDto De(Usuario usuario) => new(
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
public record LoginDto(
    string Token,
    DateTime ExpiraEm,
    UsuarioDto Usuario);

/// <summary>Agenda de um dia, com os totais por status.</summary>
public record AgendaDoDiaDto(
    DateTime Dia,
    int Total,
    int Agendados,
    int Confirmados,
    int Realizados,
    int Cancelados,
    int Faltas,
    decimal ValorPrevisto,
    List<AtendimentoResumoDto> Atendimentos);
