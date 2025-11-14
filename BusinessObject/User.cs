using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Khóa chính tự tăng (int)

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsAdmin { get; set; } = false; // Phân quyền Admin/User

        // Mối quan hệ: 1 User có nhiều Booking
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
