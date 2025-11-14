using BusinessObject;
using System.Text.Json.Serialization;

namespace MVC.Models
{
    public class LoginResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("userInfo")]
        public User UserInfo { get; set; }
    }
}
