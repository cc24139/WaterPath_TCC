namespace Domain.User
{
    using System.ComponentModel.DataAnnotations;
    using back_end.src.Domain.CorpoHidrico;

    public class User
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Senha { get; private set; }
        public string Email { get; private set; }
        public List<CorpoHidricoEntity> CorpoHidricos { get; private set; }

        public User() { }

        public User(string nome, string senha, string email)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("O nome do usuário é obrigatório.");

            if (!new EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("O email fornecido é inválido.");

            if (!VerificarSenha(senha))
                throw new ArgumentException(
                    "A senha deve conter pelo menos 6 caracteres, incluindo caracteres especiais e números."
                );

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
