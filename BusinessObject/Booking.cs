using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [StringLength(6)]
        public string BookingCode { get; set; } = string.Empty;

        // Cập nhật quan hệ: Trỏ trực tiếp đến class User
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; } // Giờ là kiểu int
        public virtual User User { get; set; } // Navigation property

        public DateTime BookingDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public int FlightId { get; set; }

        [ForeignKey("FlightId")]
        public virtual Flight? Flight { get; set; }
    }
}
