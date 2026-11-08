using System.Reflection;
using Clinica.Aplicacao.Atendimentos;
using Clinica.Aplicacao.Notificacoes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Aplicacao.Comum;

/// <summary>
/// Registro da camada de aplicação num único lugar. O Program da API chama
/// este método e não precisa conhecer handler por handler.
/// </summary>
public static class RegistroDeServicos
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection servicos)
    {
        var assembly = Assembly.GetExecutingAssembly();

        servicos.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // A ordem importa: o log envolve a validação, então uma requisição
            // recusada na validação também aparece no log com o tempo que
            // levou.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Todos os AbstractValidator do assembly, de uma vez.
        servicos.AddValidatorsFromAssembly(assembly);

        servicos.AddScoped<VerificadorDeConflitoDeAgenda>();
        servicos.AddScoped<MontadorDeMensagens>();

        return servicos;
    }
}
