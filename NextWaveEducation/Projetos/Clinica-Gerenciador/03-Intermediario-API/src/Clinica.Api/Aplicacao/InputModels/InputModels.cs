using Clinica.Api.Dominio.Enums;

namespace Clinica.Api.Aplicacao.InputModels;

/// <summary>
/// O que entra pela API. Separar isso da entidade resolve três problemas de
/// uma vez: o cliente não consegue mandar campo que não deveria (Id, Status,
/// EstaDeletado), o contrato HTTP pode mudar sem mexer no domínio, e o
/// FluentValidation tem um alvo claro para validar.
/// </summary>
public class PacienteInputModel
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DataNascimento { get; set; }
    public Sexo Sexo { get; set; }
    public string? Observacoes { get; set; }
}

public class MedicoInputModel
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Crm { get; set; } = string.Empty;
    public string UfCrm { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
}

public class ServicoInputModel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public int DuracaoMinutos { get; set; }
}

public class AgendamentoInputModel
{
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public int ServicoId { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacoes { get; set; }
}

public class RemarcacaoInputModel
{
    public DateTime NovaDataHora { get; set; }
}

public class CancelamentoInputModel
{
    public string? Motivo { get; set; }
}

public class ObservacoesInputModel
{
    public string? Observacoes { get; set; }
}

/// <summary>Anexo digitado (atestado ou receita escritos na hora).</summary>
public class AnexoInputModel
{
    public TipoAnexo Tipo { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public class UsuarioInputModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }

    /// <summary>Obrigatório quando o perfil é Medico.</summary>
    public int? MedicoId { get; set; }
}

public class LoginInputModel
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class TrocaDeSenhaInputModel
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string SenhaNova { get; set; } = string.Empty;
}
