namespace MVC.Models
{
    public class UserSessionDto
    {
        public int UserId { get; set; } // Phải là int
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAdmin  { get; set; } // Hoặc dùng bool IsAdmin nếu Role chỉ có 2 giá trị
    }
}
