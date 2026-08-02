using GerenciamentoDePessoas.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace GerenciamentoDePessoas.Models
{
    public class Pessoa
    {
        public Pessoa()
        {
        }

        public Pessoa(int id, string nome, string sobrenome, DateTime dataNascimento)
        {
            Id = id;
            Nome = nome;
            Sobrenome = sobrenome;
            DataNascimento = dataNascimento;
        }

        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório!")]
        [StringLength(10, MinimumLength =2, ErrorMessage ="O nome deve ser maior que 2 caracters e menor que 10.")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "O sobrenome é obrigatório!")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "O sobrenome deve ser maior que 2 caracters e menor que 10.")]
        public string Sobrenome { get; set; }
        
        [CustomValidation(typeof(Pessoa), "ValidarDataNascimento")]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório!")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter 11 digitos sem caracters especiais.")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "O Tipo Sanguineo é obrigatório!")]
        public ETipoSanguineo TipoSanguineo { get; set; }

        public static ValidationResult ValidarDataNascimento(DateTime dataNascimento)
        {
            if (dataNascimento > DateTime.Now)
            {
                return new ValidationResult("A data de nascimento não pode ser futura!");
            }
            return ValidationResult.Success; 
        }

    }
}
