using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;

namespace back_end.src.Infrastructure.Services
{
    public class JWTService : IJsonWebToken
    {
        private readonly string key;

        public JWTService()
        {
            key = Environment.GetEnvironmentVariable("JWT_KEY");
        }

        public string AtualizarToken(string token)
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(string username, int userId, string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(10),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public string ValidarToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
