using BusinessObject;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class BookingViewModel
    {
        public Flight FlightDetails { get; set; }
        [Required]
        public int FlightId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn hạng ghế")]
        public int SeatClassId { get; set; }
        public SelectList? SeatClasses { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string PassengerName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime PassengerDob { get; set; }
    }
}
