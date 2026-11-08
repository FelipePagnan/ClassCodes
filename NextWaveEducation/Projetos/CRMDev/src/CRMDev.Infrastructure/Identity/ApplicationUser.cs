using Microsoft.AspNetCore.Identity;

namespace CRMDev.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string Nome { get; set; } = string.Empty;
}
