namespace Infrastructure.Services;

public interface IJsonWebToken
{
    string GerarToken(string email);
    string ValidarToken(string token);
    string AtualizarToken(string token);
}