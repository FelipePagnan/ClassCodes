using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.ObjetosDeValor;
using Clinica.Api.Util;
using FluentValidation;

namespace Clinica.Api.Aplicacao.Validadores;

/// <summary>
/// Validação de formato e de forma, com FluentValidation. A divisão com o
/// domínio é esta: aqui entra o que dá para checar olhando só o que chegou
/// na requisição; o que depende do banco (CPF repetido, conflito de agenda)
/// fica no serviço, e o que é invariante da entidade fica na própria
/// entidade.
///
/// A vantagem de validar aqui primeiro é devolver todos os erros de uma vez,
/// em vez de um por requisição.
/// </summary>
public class PacienteInputModelValidator : AbstractValidator<PacienteInputModel>
{
    public PacienteInputModelValidator()
    {
        RuleFor(p => p.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(p => p.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Must(cpf => Cpf.EhValido(cpf))
            .WithMessage("O CPF é inválido (confira os dígitos verificadores).");

        RuleFor(p => p.Celular)
            .NotEmpty().WithMessage("O celular é obrigatório.")
            .Must(celular => Documentos.CelularValido(celular))
            .WithMessage("O celular é inválido. Informe DDD + 9 dígitos, ex: (11) 98765-4321.");

        RuleFor(p => p.Email)
            .Must(email => Email.EhValido(email))
            .When(p => !string.IsNullOrWhiteSpace(p.Email))
            .WithMessage("O e-mail é inválido.");

        RuleFor(p => p.DataNascimento)
            .NotEmpty().WithMessage("A data de nascimento é obrigatória.")
            .LessThanOrEqualTo(_ => DateTime.Today)
            .WithMessage("A data de nascimento não pode estar no futuro.")
            .GreaterThan(new DateTime(1900, 1, 1))
            .WithMessage("A data de nascimento informada é inválida.");

        RuleFor(p => p.Sexo).IsInEnum().WithMessage("Sexo inválido.");

        RuleFor(p => p.Observacoes)
            .MaximumLength(1000).WithMessage("As observações devem ter no máximo 1000 caracteres.");
    }
}

public class MedicoInputModelValidator : AbstractValidator<MedicoInputModel>
{
    public MedicoInputModelValidator()
    {
        RuleFor(m => m.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(m => m.Crm)
            .NotEmpty().WithMessage("O CRM é obrigatório.")
            .Must(crm => Documentos.CrmValido(crm))
            .WithMessage("O CRM é inválido. Informe de 4 a 7 dígitos.");

        RuleFor(m => m.UfCrm)
            .NotEmpty().WithMessage("A UF do CRM é obrigatória.")
            .Must(uf => Documentos.UfValida(uf))
            .WithMessage("A UF é inválida. Use a sigla do estado, ex: SP.");

        RuleFor(m => m.Especialidade)
            .NotEmpty().WithMessage("A especialidade é obrigatória.")
            .MaximumLength(120).WithMessage("A especialidade deve ter no máximo 120 caracteres.");

        RuleFor(m => m.Email)
            .Must(email => Email.EhValido(email))
            .When(m => !string.IsNullOrWhiteSpace(m.Email))
            .WithMessage("O e-mail é inválido.");

        RuleFor(m => m.Celular)
            .Must(celular => Documentos.CelularValido(celular!))
            .When(m => !string.IsNullOrWhiteSpace(m.Celular))
            .WithMessage("O celular é inválido.");
    }
}

public class ServicoInputModelValidator : AbstractValidator<ServicoInputModel>
{
    public ServicoInputModelValidator()
    {
        RuleFor(s => s.Nome)
            .NotEmpty().WithMessage("O nome do serviço é obrigatório.")
            .MinimumLength(3).WithMessage("O nome do serviço deve ter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome do serviço deve ter no máximo 150 caracteres.");

        RuleFor(s => s.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(s => s.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor não pode ser negativo.")
            // Round compara por valor, não por escala: 250.00 passa, 250.005 não.
            .Must(valor => decimal.Round(valor, 2) == valor)
            .WithMessage("O valor deve ter no máximo 2 casas decimais.");

        RuleFor(s => s.DuracaoMinutos)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero.")
            .LessThanOrEqualTo(Servico.DuracaoMaximaMinutos)
            .WithMessage($"A duração não pode passar de {Servico.DuracaoMaximaMinutos} minutos.");
    }
}

public class AgendamentoInputModelValidator : AbstractValidator<AgendamentoInputModel>
{
    public AgendamentoInputModelValidator()
    {
        RuleFor(a => a.PacienteId).GreaterThan(0).WithMessage("Informe o paciente.");
        RuleFor(a => a.MedicoId).GreaterThan(0).WithMessage("Informe o médico.");
        RuleFor(a => a.ServicoId).GreaterThan(0).WithMessage("Informe o serviço.");

        RuleFor(a => a.DataHora)
            .NotEmpty().WithMessage("A data e hora são obrigatórias.")
            .GreaterThan(_ => DateTime.Now)
            .WithMessage("Não é possível agendar para um horário que já passou.")
            .Must(data => data.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("A clínica não atende aos domingos.")
            .Must(data => data.TimeOfDay >= Atendimento.AberturaClinica
                          && data.TimeOfDay < Atendimento.FechamentoClinica)
            .WithMessage($"A clínica atende das {Atendimento.AberturaClinica:hh\\:mm} " +
                         $"às {Atendimento.FechamentoClinica:hh\\:mm}.");

        RuleFor(a => a.Observacoes)
            .MaximumLength(1000).WithMessage("As observações devem ter no máximo 1000 caracteres.");
    }
}

public class RemarcacaoInputModelValidator : AbstractValidator<RemarcacaoInputModel>
{
    public RemarcacaoInputModelValidator()
    {
        RuleFor(r => r.NovaDataHora)
            .NotEmpty().WithMessage("A nova data e hora são obrigatórias.")
            .GreaterThan(_ => DateTime.Now)
            .WithMessage("Não é possível remarcar para um horário que já passou.")
            .Must(data => data.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("A clínica não atende aos domingos.");
    }
}

public class AnexoInputModelValidator : AbstractValidator<AnexoInputModel>
{
    public AnexoInputModelValidator()
    {
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("O tipo do anexo deve ser Atestado, Receita ou Evolucao.");

        RuleFor(x => x.NomeArquivo)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .MaximumLength(255).WithMessage("O nome do arquivo deve ter no máximo 255 caracteres.");

        RuleFor(x => x.Conteudo)
            .NotEmpty().WithMessage("O conteúdo do documento é obrigatório.")
            .Must(conteudo =>
                System.Text.Encoding.UTF8.GetByteCount(conteudo) <= Anexo.TamanhoMaximoBytes)
            .WithMessage($"O anexo deve ter no máximo {Anexo.TamanhoMaximoBytes / 1024 / 1024}MB.");

        RuleFor(x => x.Descricao)
            .MaximumLength(4000).WithMessage("A descrição deve ter no máximo 4000 caracteres.");
    }
}

public class UsuarioInputModelValidator : AbstractValidator<UsuarioInputModel>
{
    public UsuarioInputModelValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .Must(email => Email.EhValido(email)).WithMessage("O e-mail é inválido.");

        RuleFor(u => u.Senha)
            .Must(senha => Senhas.SenhaForte(senha, out _))
            .WithMessage("A senha deve ter ao menos 8 caracteres, com letra e número.");

        RuleFor(u => u.Perfil)
            .IsInEnum()
            .WithMessage("O perfil deve ser Administrador, Medico ou Recepcionista.");

        // Perfil Médico precisa de vínculo; os outros não aceitam.
        RuleFor(u => u.MedicoId)
            .NotNull()
            .When(u => u.Perfil == Dominio.Enums.PerfilUsuario.Medico)
            .WithMessage("Um usuário com perfil Médico precisa do Id do médico vinculado.");

        RuleFor(u => u.MedicoId)
            .Null()
            .When(u => u.Perfil != Dominio.Enums.PerfilUsuario.Medico)
            .WithMessage("Só o perfil Médico aceita vínculo com cadastro de médico.");
    }
}

public class LoginInputModelValidator : AbstractValidator<LoginInputModel>
{
    public LoginInputModelValidator()
    {
        RuleFor(l => l.Email).NotEmpty().WithMessage("Informe o e-mail.");
        RuleFor(l => l.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}

public class TrocaDeSenhaInputModelValidator : AbstractValidator<TrocaDeSenhaInputModel>
{
    public TrocaDeSenhaInputModelValidator()
    {
        RuleFor(t => t.SenhaAtual).NotEmpty().WithMessage("Informe a senha atual.");

        RuleFor(t => t.SenhaNova)
            .Must(senha => Senhas.SenhaForte(senha, out _))
            .WithMessage("A nova senha deve ter ao menos 8 caracteres, com letra e número.");
    }
}
