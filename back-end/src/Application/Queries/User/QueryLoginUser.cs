using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace back_end.src.Application.Queries.User
{

    public class UserLoginResponse
    {
        public string token { get; set; }
        public string email { get; set; }
        public string nome { get; set; }
    }

    public class QueryLoginUser : IRequest<UserLoginResponse>
    {
        public string email { get; set; }
        public string senha { get; set; }
    }
}