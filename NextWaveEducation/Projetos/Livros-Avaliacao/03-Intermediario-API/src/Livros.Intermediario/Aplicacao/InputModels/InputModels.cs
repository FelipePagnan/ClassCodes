using Livros.Intermediario.Dominio.Enums;

namespace Livros.Intermediario.Aplicacao.InputModels;

/// <summary>Modelo de entrada para cadastro de livro.</summary>
public class CriarLivroInputModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public GeneroLivroEnum Genero { get; set; }
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }
}

/// <summary>Modelo de entrada para atualização de livro.</summary>
public class AtualizarLivroInputModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public GeneroLivroEnum Genero { get; set; }
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }
}

/// <summary>Modelo de entrada para cadastro de usuário.</summary>
public class CriarUsuarioInputModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>Modelo de entrada para atualização de usuário.</summary>
public class AtualizarUsuarioInputModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>Modelo de entrada para cadastro de avaliação de um livro.</summary>
public class CriarAvaliacaoInputModel
{
    public int IdUsuario { get; set; }
    public int Nota { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicioLeitura { get; set; }
    public DateTime DataFimLeitura { get; set; }
}

/// <summary>Modelo de entrada para importar um livro a partir da API externa.</summary>
public class ImportarLivroInputModel
{
    public string Isbn { get; set; } = string.Empty;
    public GeneroLivroEnum? Genero { get; set; }
}
