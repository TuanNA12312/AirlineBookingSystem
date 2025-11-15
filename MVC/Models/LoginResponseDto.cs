using BusinessObject;
using System.Text.Json.Serialization;

namespace MVC.Models
{
    public class LoginResponseDto
    {
        // PHẢI dùng tên UserInfo để khớp với API và Session!
        public UserSessionDto UserInfo { get; set; }
        public string Token { get; set; }
    }
}
