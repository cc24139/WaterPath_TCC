using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Infrastructure.Services.Interfaces;
using BCrypt;

namespace back_end.src.Infrastructure.Services
{

    public class HashServices : IPasswordHash
    {
        public string ComputeHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyHash(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}