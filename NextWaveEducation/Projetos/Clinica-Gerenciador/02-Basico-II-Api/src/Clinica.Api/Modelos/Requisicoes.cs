using Clinica.Api.Enums;

namespace Clinica.Api.Modelos;

/// <summary>
/// Modelos de entrada do nível Básico II. Aqui eles são só o mínimo para a
/// API não receber a entidade crua onde isso seria perigoso (senha, ids de
/// relacionamento). A separação completa em InputModel / ViewModel / DTO
/// chega no nível Intermediário.
/// </summary>
public record PacienteRequisicao(
    string NomeCompleto,
    string Cpf,
    string Celular,
    string? Email,
    DateTime DataNascimento,
    Sexo Sexo,
    string? Observacoes);

public record MedicoRequisicao(
    string NomeCompleto,
    string Crm,
    string UfCrm,
    string Especialidade,
    string? Email,
    string? Celular);

public record ServicoRequisicao(
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos);

public record AgendamentoRequisicao(
    int PacienteId,
    int MedicoId,
    int ServicoId,
    DateTime DataHora,
    string? Observacoes);

public record RemarcacaoRequisicao(DateTime NovaDataHora);

public record CancelamentoRequisicao(string? Motivo);

public record AnexoRequisicao(
    TipoAnexo Tipo,
    string NomeArquivo,
    string Conteudo,
    string? Descricao);

public record UsuarioRequisicao(
    string Nome,
    string Email,
    string Senha,
    PerfilUsuario Perfil,
    int? MedicoId);

public record TrocaDeSenhaRequisicao(string SenhaAtual, string SenhaNova);
