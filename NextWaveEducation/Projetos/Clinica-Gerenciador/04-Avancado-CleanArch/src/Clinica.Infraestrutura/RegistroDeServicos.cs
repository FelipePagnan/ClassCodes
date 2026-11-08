using Clinica.Aplicacao.Comum;
using Clinica.Aplicacao.Contratos;
using Clinica.Dominio.Repositorios;
using Clinica.Infraestrutura.Calendario;
using Clinica.Infraestrutura.Notificacoes;
using Clinica.Infraestrutura.Persistencia;
using Clinica.Infraestrutura.Persistencia.Repositorios;
using Clinica.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Infraestrutura;

/// <summary>
/// Registro da infraestrutura. É aqui que cada contrato declarado no domínio
/// e na aplicação ganha uma implementação concreta — e é o único lugar do
/// sistema que conhece Entity Framework, SQLite e JWT ao mesmo tempo.
/// </summary>
public static class RegistroDeServicos
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.Configure<JwtConfiguracao>(configuracao.GetSection(JwtConfiguracao.Secao));

        servicos.AddDbContext<ClinicaDbContext>(opcoes =>
            opcoes.UseSqlite(
                configuracao.GetConnectionString("Clinica") ?? "Data Source=clinica.db"));

        // Repositórios: contratos do domínio, implementação com EF Core.
        servicos.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
        servicos.AddScoped<IMedicoRepositorio, MedicoRepositorio>();
        servicos.AddScoped<IServicoRepositorio, ServicoRepositorio>();
        servicos.AddScoped<IAtendimentoRepositorio, AtendimentoRepositorio>();
        servicos.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        // Unit of Work e publicação de eventos.
        servicos.AddScoped<IUnitOfWork, UnitOfWork>();
        servicos.AddScoped<IDespachanteDeEventos, DespachanteDeEventosMediatR>();

        // Contratos da aplicação.
        servicos.AddScoped<IGeradorDeToken, GeradorDeTokenJwt>();
        servicos.AddSingleton<IGeradorDeConviteDeCalendario, GeradorDeConviteIcs>();

        // Singleton: a caixa de saída guarda estado entre requisições, e o
        // background service escreve nela também.
        servicos.AddSingleton<IEnviadorDeNotificacao, CaixaDeSaidaEmMemoria>();

        return servicos;
    }

    /// <summary>Cria o banco e carrega os dados de exemplo.</summary>
    public static async Task PrepararBancoAsync(this IServiceProvider provider)
    {
        using var escopo = provider.CreateScope();

        var context = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        await DadosIniciais.SemearAsync(context);
    }
}
