using Clinica.Api.Entidades;
using Clinica.Api.Enums;
using Clinica.Api.Util;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Data;

/// <summary>
/// Semeia a base com dados de exemplo. Como o provider é InMemory, isso roda
/// a cada inicialização — senão a API subiria vazia e não haveria como testar.
/// </summary>
public static class DadosIniciais
{
    private const string SenhaPadrao = "Clinica123";

    public static async Task SemearAsync(ClinicaDbContext context)
    {
        if (await context.Pacientes.AnyAsync()) return;

        // ------------------------------- Serviços -------------------------------
        var consulta = new Servico
        {
            Nome = "Consulta clínica",
            Descricao = "Consulta eletiva em consultório",
            Valor = 250.00m,
            DuracaoMinutos = 30
        };

        var retorno = new Servico
        {
            Nome = "Retorno",
            Descricao = "Revisão de exames e conduta",
            Valor = 120.00m,
            DuracaoMinutos = 20
        };

        var eletro = new Servico
        {
            Nome = "Eletrocardiograma",
            Descricao = "ECG de repouso com laudo",
            Valor = 180.00m,
            DuracaoMinutos = 25
        };

        context.Servicos.AddRange(consulta, retorno, eletro);

        // ------------------------------- Médicos -------------------------------
        var cardiologista = new Medico
        {
            NomeCompleto = "Helena Vasconcelos",
            Crm = "123456",
            UfCrm = "SP",
            Especialidade = "Cardiologia",
            Email = "helena.vasconcelos@clinicavida.com.br"
        };

        var dermatologista = new Medico
        {
            NomeCompleto = "Rafael Queiroz",
            Crm = "98765",
            UfCrm = "SP",
            Especialidade = "Dermatologia",
            Email = "rafael.queiroz@clinicavida.com.br"
        };

        context.Medicos.AddRange(cardiologista, dermatologista);

        // ------------------------------ Pacientes ------------------------------
        // CPFs com dígitos verificadores válidos.
        var mariana = new Paciente
        {
            NomeCompleto = "Mariana Lopes Ferreira",
            Cpf = "52998224725",
            Celular = "11987654321",
            Email = "mariana.ferreira@exemplo.com.br",
            DataNascimento = new DateTime(1988, 4, 17),
            Sexo = Sexo.Feminino
        };

        var bruno = new Paciente
        {
            NomeCompleto = "Bruno Tavares Alencar",
            Cpf = "11144477735",
            Celular = "11912345678",
            Email = "bruno.alencar@exemplo.com.br",
            DataNascimento = new DateTime(1975, 11, 2),
            Sexo = Sexo.Masculino
        };

        var livia = new Paciente
        {
            NomeCompleto = "Lívia Tavares Alencar",
            Cpf = "16899535009",
            // Mesmo celular do pai: a busca por celular devolve os dois.
            Celular = "11912345678",
            DataNascimento = DateTime.Today.AddYears(-9),
            Sexo = Sexo.Feminino,
            Observacoes = "Menor de idade. Responsável: Bruno Tavares Alencar."
        };

        context.Pacientes.AddRange(mariana, bruno, livia);

        await context.SaveChangesAsync();

        // ------------------------------- Usuários -------------------------------
        var (hashAdmin, saltAdmin) = Senhas.Gerar(SenhaPadrao);
        var (hashRecepcao, saltRecepcao) = Senhas.Gerar(SenhaPadrao);
        var (hashMedica, saltMedica) = Senhas.Gerar(SenhaPadrao);

        context.Usuarios.AddRange(
            new Usuario
            {
                Nome = "Administração",
                Email = "admin@clinicavida.com.br",
                SenhaHash = hashAdmin,
                SenhaSalt = saltAdmin,
                Perfil = PerfilUsuario.Administrador
            },
            new Usuario
            {
                Nome = "Recepção",
                Email = "recepcao@clinicavida.com.br",
                SenhaHash = hashRecepcao,
                SenhaSalt = saltRecepcao,
                Perfil = PerfilUsuario.Recepcionista
            },
            new Usuario
            {
                Nome = "Helena Vasconcelos",
                Email = "helena@clinicavida.com.br",
                SenhaHash = hashMedica,
                SenhaSalt = saltMedica,
                Perfil = PerfilUsuario.Medico,
                MedicoId = cardiologista.Id
            });

        // ------------------------------- Agenda -------------------------------
        // Amanhã (para o background service ter o que notificar) e depois de
        // amanhã, sempre em dia útil e dentro do horário de funcionamento.
        var amanha = ProximoDiaUtil(DateTime.Today.AddDays(1)).AddHours(9);
        var depois = ProximoDiaUtil(DateTime.Today.AddDays(2)).AddHours(14);

        context.Atendimentos.AddRange(
            new Atendimento
            {
                PacienteId = mariana.Id,
                MedicoId = cardiologista.Id,
                ServicoId = consulta.Id,
                DataHora = amanha,
                DuracaoMinutos = consulta.DuracaoMinutos,
                Valor = consulta.Valor,
                Status = StatusAtendimento.Agendado,
                Observacoes = "Primeira consulta. Levar exames anteriores."
            },
            new Atendimento
            {
                PacienteId = bruno.Id,
                MedicoId = cardiologista.Id,
                ServicoId = retorno.Id,
                DataHora = amanha.AddMinutes(consulta.DuracaoMinutos),
                DuracaoMinutos = retorno.DuracaoMinutos,
                Valor = retorno.Valor,
                Status = StatusAtendimento.Agendado
            },
            new Atendimento
            {
                PacienteId = mariana.Id,
                MedicoId = dermatologista.Id,
                ServicoId = consulta.Id,
                DataHora = depois,
                DuracaoMinutos = consulta.DuracaoMinutos,
                Valor = consulta.Valor,
                Status = StatusAtendimento.Agendado
            });

        await context.SaveChangesAsync();
    }

    /// <summary>Pula o domingo, já que a clínica não atende nesse dia.</summary>
    private static DateTime ProximoDiaUtil(DateTime data)
        => data.DayOfWeek == DayOfWeek.Sunday ? data.AddDays(1) : data;
}
