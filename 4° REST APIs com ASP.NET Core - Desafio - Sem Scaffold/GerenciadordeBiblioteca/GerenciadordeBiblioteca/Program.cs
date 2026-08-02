using GerenciadordeBiblioteca.Data;
using GerenciadordeBiblioteca.Repository;
using GerenciadordeBiblioteca.Services;
using Microsoft.EntityFrameworkCore;

namespace GerenciadordeBiblioteca
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<GerenciadordeBibliotecaContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("GerenciadordeBibliotecaContext")));


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();
            builder.Services.AddScoped<IBibliotecaRepository, BibliotecaRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
