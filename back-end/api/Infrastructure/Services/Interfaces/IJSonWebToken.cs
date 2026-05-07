namespace Infrastructure.Services;

public interface IJsonWebToken
{

    string GenerateToken(string username, int userId, string email);
    string ValidarToken(string token);
    string AtualizarToken(string token);
}