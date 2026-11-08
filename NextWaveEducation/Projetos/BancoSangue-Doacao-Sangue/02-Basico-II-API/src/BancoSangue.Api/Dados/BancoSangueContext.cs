using BancoSangue.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Api.Dados;

public class BancoSangueContext : DbContext
{
    public BancoSangueContext(DbContextOptions<BancoSangueContext> options) : base(options) { }

    public DbSet<Doador> Doadores => Set<Doador>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<EstoqueSangue> EstoqueSangue => Set<EstoqueSangue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doador>()
            .HasIndex(d => d.Email)
            .IsUnique();

        modelBuilder.Entity<Doador>()
            .HasMany(d => d.Doacoes)
            .WithOne(d => d.Doador)
            .HasForeignKey(d => d.DoadorId);

        modelBuilder.Entity<Doador>()
            .HasOne(d => d.Endereco)
            .WithOne(e => e.Doador)
            .HasForeignKey<Endereco>(e => e.DoadorId);

        // Propriedades calculadas não têm coluna no banco.
        modelBuilder.Entity<Doador>().Ignore(d => d.Idade);
        modelBuilder.Entity<Doador>().Ignore(d => d.EhMaiorDeIdade);
        modelBuilder.Entity<Doador>().Ignore(d => d.UltimaDoacao);
        modelBuilder.Entity<Doador>().Ignore(d => d.IntervaloMinimoDiasEntreDoacoes);
        modelBuilder.Entity<EstoqueSangue>().Ignore(e => e.EstoqueBaixo);

        // Semeia o estoque com todos os tipos sanguíneos possíveis, zerados.
        var id = 1;
        var seed = new List<EstoqueSangue>();
        foreach (Enums.TipoSanguineo tipo in Enum.GetValues<Enums.TipoSanguineo>())
            foreach (Enums.FatorRh fator in Enum.GetValues<Enums.FatorRh>())
                seed.Add(new EstoqueSangue { Id = id++, TipoSanguineo = tipo, FatorRh = fator, QuantidadeML = 0 });

        modelBuilder.Entity<EstoqueSangue>().HasData(seed);
    }
}
