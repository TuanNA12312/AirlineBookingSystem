using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class ChangePasswordViewModel
    {
        // Phải có UserId để truyền vào DTO cho API
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}