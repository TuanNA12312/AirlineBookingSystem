using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } // Trạng thái đặt chỗ
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public string PNR { get; set; } // Mã đặt chỗ

        // Thông tin Chuyến bay
        public Flight? FlightInfo { get; set; } // Có thể dùng lại Entity Flight

        // Danh sách các vé/hành khách
        public ICollection<Ticket>? Tickets { get; set; } // Có thể dùng lại Entity Ticket
    }
}
