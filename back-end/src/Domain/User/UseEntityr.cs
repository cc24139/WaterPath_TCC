namespace Domain.User
{
    using System.ComponentModel.DataAnnotations;
    public class User
    {
public int Id { get; private set; }
public string Nome { get; private set; }
public string Senha { get; private set; }
public string Email { get; private set; }

        public User()
        {
            Id = 0;
            Nome = string.Empty;
            Senha = string.Empty;
            Email = string.Empty;
        }

        public void AdicionarUsuario(string nome, string senha, string email)
        {
            if (!VerificarSenha(senha))
            {
                throw new ArgumentException("Senha inválida");
            }
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Email inválido");
            }
            Nome = nome;
            Senha = senha;
            Email = email;
        }

        private bool VerificarSenha(string senha)
        {
            char[] caracFortes = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '+' };
            char[] nums = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            bool numCaract = senha.Length >= 6;
            bool temCaracFortes = caracFortes.Any(c => senha.Contains(c));
            bool temNumeros = nums.Any(c => senha.Contains(c));

            return !string.IsNullOrEmpty(senha) && numCaract && temCaracFortes && temNumeros;
        }

    }
}