using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Interfaces;

namespace Livros.Intermediario.Aplicacao.ViewModels;

// ------------------------------- LIVROS -------------------------------

public class LivroViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }
    public decimal NotaMedia { get; set; }
    public int TotalAvaliacoes { get; set; }
    public bool TemCapa { get; set; }
    public DateTime DataCriacao { get; set; }

    public static LivroViewModel De(Livro l) => new()
    {
        Id = l.Id,
        Titulo = l.Titulo,
        Autor = l.Autor,
        Editora = l.Editora,
        ISBN = l.ISBN,
        Genero = l.Genero.ToString(),
        AnoDePublicacao = l.AnoDePublicacao,
        QuantidadePaginas = l.QuantidadePaginas,
        NotaMedia = l.NotaMedia,
        TotalAvaliacoes = l.Avaliacoes.Count,
        TemCapa = l.TemCapa,
        DataCriacao = l.DataCriacao
    };
}

public class LivroDetalheViewModel : LivroViewModel
{
    public string Descricao { get; set; } = string.Empty;
    public List<AvaliacaoViewModel> Avaliacoes { get; set; } = new();

    /// <summary>Quantas avaliações o livro recebeu em cada nota de 1 a 5.</summary>
    public Dictionary<int, int> DistribuicaoDeNotas { get; set; } = new();

    public static LivroDetalheViewModel DeDetalhe(Livro l)
    {
        var baseVm = De(l);

        return new LivroDetalheViewModel
        {
            Id = baseVm.Id,
            Titulo = baseVm.Titulo,
            Autor = baseVm.Autor,
            Editora = baseVm.Editora,
            ISBN = baseVm.ISBN,
            Genero = baseVm.Genero,
            AnoDePublicacao = baseVm.AnoDePublicacao,
            QuantidadePaginas = baseVm.QuantidadePaginas,
            NotaMedia = baseVm.NotaMedia,
            TotalAvaliacoes = baseVm.TotalAvaliacoes,
            TemCapa = baseVm.TemCapa,
            DataCriacao = baseVm.DataCriacao,
            Descricao = l.Descricao,
            Avaliacoes = l.Avaliacoes
                .OrderByDescending(a => a.DataCriacao)
                .Select(AvaliacaoViewModel.De)
                .ToList(),
            DistribuicaoDeNotas = Enumerable.Range(Avaliacao.NotaMinima, Avaliacao.NotaMaxima)
                .ToDictionary(nota => nota, nota => l.Avaliacoes.Count(a => a.Nota == nota))
        };
    }
}

// ------------------------------ USUÁRIOS ------------------------------

public class UsuarioViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalAvaliacoes { get; set; }
    public double? NotaMediaAtribuida { get; set; }
    public DateTime DataCriacao { get; set; }

    public static UsuarioViewModel De(Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        TotalAvaliacoes = u.TotalAvaliacoes,
        NotaMediaAtribuida = u.NotaMediaAtribuida,
        DataCriacao = u.DataCriacao
    };
}

public class UsuarioDetalheViewModel : UsuarioViewModel
{
    public List<AvaliacaoDoUsuarioViewModel> Avaliacoes { get; set; } = new();
    public int TotalPaginasLidas { get; set; }

    public static UsuarioDetalheViewModel DeDetalhe(Usuario u)
    {
        var baseVm = De(u);

        return new UsuarioDetalheViewModel
        {
            Id = baseVm.Id,
            Nome = baseVm.Nome,
            Email = baseVm.Email,
            TotalAvaliacoes = baseVm.TotalAvaliacoes,
            NotaMediaAtribuida = baseVm.NotaMediaAtribuida,
            DataCriacao = baseVm.DataCriacao,
            Avaliacoes = u.Avaliacoes
                .OrderByDescending(a => a.DataFimLeitura)
                .Select(AvaliacaoDoUsuarioViewModel.De)
                .ToList(),
            TotalPaginasLidas = u.Avaliacoes.Sum(a => a.Livro?.QuantidadePaginas ?? 0)
        };
    }
}

// ----------------------------- AVALIAÇÕES -----------------------------

public class AvaliacaoViewModel
{
    public int Id { get; set; }
    public int Nota { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public string? NomeUsuario { get; set; }
    public DateTime DataInicioLeitura { get; set; }
    public DateTime DataFimLeitura { get; set; }
    public int DiasDeLeitura { get; set; }
    public DateTime DataCriacao { get; set; }

    public static AvaliacaoViewModel De(Avaliacao a) => new()
    {
        Id = a.Id,
        Nota = a.Nota,
        Descricao = a.Descricao,
        IdUsuario = a.IdUsuario,
        NomeUsuario = a.Usuario?.Nome,
        DataInicioLeitura = a.DataInicioLeitura,
        DataFimLeitura = a.DataFimLeitura,
        DiasDeLeitura = a.DiasDeLeitura,
        DataCriacao = a.DataCriacao
    };
}

public class AvaliacaoDoUsuarioViewModel
{
    public int Id { get; set; }
    public int Nota { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int IdLivro { get; set; }
    public string? TituloLivro { get; set; }
    public string? AutorLivro { get; set; }
    public DateTime DataInicioLeitura { get; set; }
    public DateTime DataFimLeitura { get; set; }
    public int DiasDeLeitura { get; set; }

    public static AvaliacaoDoUsuarioViewModel De(Avaliacao a) => new()
    {
        Id = a.Id,
        Nota = a.Nota,
        Descricao = a.Descricao,
        IdLivro = a.IdLivro,
        TituloLivro = a.Livro?.Titulo,
        AutorLivro = a.Livro?.Autor,
        DataInicioLeitura = a.DataInicioLeitura,
        DataFimLeitura = a.DataFimLeitura,
        DiasDeLeitura = a.DiasDeLeitura
    };
}

/// <summary>Resultado do cadastro de avaliação, com a nova nota média do livro.</summary>
public class ResultadoAvaliacaoViewModel
{
    public AvaliacaoViewModel Avaliacao { get; set; } = new();
    public string TituloLivro { get; set; } = string.Empty;
    public decimal NotaMediaAtualizada { get; set; }
    public int TotalAvaliacoesDoLivro { get; set; }
}

// ----------------------------- RELATÓRIOS -----------------------------

public class ItemRelatorioAnualViewModel
{
    public int IdLivro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public int QuantidadePaginas { get; set; }
    public int QuantidadeLeituras { get; set; }
    public double NotaMedia { get; set; }
}

public class RelatorioAnualViewModel
{
    public int Ano { get; set; }
    public int LivrosDistintosLidos { get; set; }
    public int TotalDeLeituras { get; set; }
    public int PaginasLidas { get; set; }
    public int LeitoresDistintos { get; set; }
    public double? NotaMediaGeral { get; set; }
    public List<ItemRelatorioAnualViewModel> Livros { get; set; } = new();
}

// -------------------------- CONSULTA EXTERNA --------------------------

public class LivroExternoViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }

    /// <summary>Indica se esse ISBN já está cadastrado na base local.</summary>
    public bool JaCadastrado { get; set; }

    public static LivroExternoViewModel De(LivroExternoDto dto, bool jaCadastrado) => new()
    {
        Titulo = dto.Titulo,
        Descricao = dto.Descricao,
        Isbn = dto.Isbn,
        Autor = dto.Autor,
        Editora = dto.Editora,
        Categoria = dto.Categoria,
        AnoDePublicacao = dto.AnoDePublicacao,
        QuantidadePaginas = dto.QuantidadePaginas,
        JaCadastrado = jaCadastrado
    };
}
