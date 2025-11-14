using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public User UserInfo { get; set; }
    }
}
