using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
