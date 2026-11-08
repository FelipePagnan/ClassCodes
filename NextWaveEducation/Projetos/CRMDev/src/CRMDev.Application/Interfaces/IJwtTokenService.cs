namespace CRMDev.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(string userId, string nome, string email);
}
