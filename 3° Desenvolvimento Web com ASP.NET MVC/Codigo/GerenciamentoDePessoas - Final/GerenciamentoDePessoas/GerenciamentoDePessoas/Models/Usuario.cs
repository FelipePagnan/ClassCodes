using Microsoft.AspNetCore.Identity;

namespace GerenciamentoDePessoas.Models
{
    public class Usuario : IdentityUser
    {
        public string CPF { get; set; }
    }
}
