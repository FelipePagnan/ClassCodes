using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enums;
using Clinica.Dominio.ObjetosDeValor;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestrutura.Persistencia;

/// <summary>
/// Cria o banco e semeia dados de exemplo. Roda só quando a base está vazia:
/// com SQLite o arquivo persiste entre execuções.
///
/// Repare que aqui se usa SaveChangesAsync direto, e não o IUnitOfWork. É de
/// propósito: a carga inicial não deve disparar os eventos de domínio, senão
/// a aplicação subiria mandando confirmação de atendimento para os pacientes
/// de exemplo. Os eventos acumulados são descartados no fim.
/// </summary>
public static class DadosIniciais
{
    private const string SenhaPadrao = "Clinica123";

    public static async Task SemearAsync(ClinicaDbContext context)
    {
        // Em projeto real a criação do schema vem de migrations
        // (dotnet ef migrations add / database update). EnsureCreated mantém
        // o foco no que o nível quer mostrar.
        await context.Database.EnsureCreatedAsync();

        if (await context.Pacientes.AnyAsync()) return;

        // ------------------------------- Serviços -------------------------------
        var consulta = new Servico("Consulta clínica", 250.00m, 30,
            "Consulta eletiva em consultório");

        var retorno = new Servico("Retorno", 120.00m, 20,
            "Revisão de exames e conduta");

        var eletro = new Servico("Eletrocardiograma", 180.00m, 25,
            "ECG de repouso com laudo");

        context.Servicos.AddRange(consulta, retorno, eletro);

        // ------------------------------- Médicos -------------------------------
        var cardiologista = new Medico(
            "Helena Vasconcelos",
            Crm.Criar("123456", "SP"),
            "Cardiologia",
            Email.Criar("helena.vasconcelos@clinicavida.com.br"));

        var dermatologista = new Medico(
            "Rafael Queiroz",
            Crm.Criar("98765", "SP"),
            "Dermatologia",
            Email.Criar("rafael.queiroz@clinicavida.com.br"));

        context.Medicos.AddRange(cardiologista, dermatologista);

        // ------------------------------ Pacientes ------------------------------
        // CPFs com dígitos verificadores válidos.
        var mariana = new Paciente(
            "Mariana Lopes Ferreira",
            Cpf.Criar("529.982.247-25"),
            Celular.Criar("(11) 98765-4321"),
            new DateTime(1988, 4, 17),
            Sexo.Feminino,
            Email.Criar("mariana.ferreira@exemplo.com.br"));

        var bruno = new Paciente(
            "Bruno Tavares Alencar",
            Cpf.Criar("111.444.777-35"),
            Celular.Criar("(11) 91234-5678"),
            new DateTime(1975, 11, 2),
            Sexo.Masculino,
            Email.Criar("bruno.alencar@exemplo.com.br"));

        // Mesmo celular do pai: a busca por celular devolve os dois.
        var livia = new Paciente(
            "Lívia Tavares Alencar",
            Cpf.Criar("168.995.350-09"),
            Celular.Criar("(11) 91234-5678"),
            DateTime.Today.AddYears(-9),
            Sexo.Feminino,
            null,
            "Menor de idade. Responsável: Bruno Tavares Alencar.");

        context.Pacientes.AddRange(mariana, bruno, livia);

        // Salva aqui porque os atendimentos e o usuário da médica precisam
        // dos Ids já gerados.
        await context.SaveChangesAsync();

        // ------------------------------- Usuários -------------------------------
        context.Usuarios.AddRange(
            new Usuario(
                "Administração",
                Email.Criar("admin@clinicavida.com.br"),
                SenhaPadrao,
                PerfilUsuario.Administrador),
            new Usuario(
                "Recepção",
                Email.Criar("recepcao@clinicavida.com.br"),
                SenhaPadrao,
                PerfilUsuario.Recepcionista),
            new Usuario(
                "Helena Vasconcelos",
                Email.Criar("helena@clinicavida.com.br"),
                SenhaPadrao,
                PerfilUsuario.Medico,
                cardiologista));

        // -------------------------------- Agenda --------------------------------
        // Amanhã (para o background service ter o que notificar) e depois de
        // amanhã, sempre em dia útil e dentro do horário de funcionamento.
        var amanha = ProximoDiaUtil(DateTime.Today.AddDays(1)).AddHours(9);
        var depois = ProximoDiaUtil(DateTime.Today.AddDays(2)).AddHours(14);

        context.Atendimentos.AddRange(
            new Atendimento(mariana, cardiologista, consulta, amanha,
                "Primeira consulta. Levar exames anteriores."),
            new Atendimento(bruno, cardiologista, retorno,
                amanha.AddMinutes(consulta.DuracaoMinutos)),
            new Atendimento(mariana, dermatologista, consulta, depois));

        await context.SaveChangesAsync();

        // Descarta os eventos que a carga inicial gerou, para que nenhum
        // CommitAsync posterior neste mesmo escopo os publique.
        foreach (var entrada in context.ChangeTracker.Entries<EntidadeBase>())
            entrada.Entity.LimparEventos();
    }

    /// <summary>Pula o domingo, já que a clínica não atende nesse dia.</summary>
    private static DateTime ProximoDiaUtil(DateTime data)
        => data.DayOfWeek == DayOfWeek.Sunday ? data.AddDays(1) : data;
}
