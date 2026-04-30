using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Infrastructure.Services.Interfaces
{
    public interface IPasswordHash 
    {
        string ComputeHash(string password);
        bool VerifyHash(string password, string hash);
    }
}