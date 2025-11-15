using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class UserProfileUpdateViewModel
    {
        // Khóa chính là int
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        [Display(Name = "Tên đầy đủ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}
