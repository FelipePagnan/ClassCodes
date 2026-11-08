namespace CRMDev.Application.DTOs;

public record RegistrarUsuarioDto(string Nome, string Email, string Senha);

public record LoginDto(string Email, string Senha);

public record AuthResponseDto(string Token, DateTime ExpiraEm, string Nome, string Email);
