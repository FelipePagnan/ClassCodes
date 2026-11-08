using System.ComponentModel.DataAnnotations;

namespace GerenciadordeBiblioteca.Models
{
    public class Livro
    {
        public Livro() { }

        public Livro(int id, string titulo, string autor, string isbn, int anoPublicacao)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            ISBN = isbn;
            AnoPublicacao = anoPublicacao;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "O Titulo é obrigatório")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "O Autor é obrigatório")]
        public string Autor { get; set; }

        [Required(ErrorMessage = "O ISBN é obrigatório")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "O Ano de Publicação é obrigatório")]
        [Range(1000, 9999, ErrorMessage = "O Ano deve ter 4 dígitos")]
        public int AnoPublicacao { get; set; }

    }
}
