using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [ForeignKey("Flight")]
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        [ForeignKey("Passenger")]
        public int PassengerId { get; set; }
        public virtual Passenger Passenger { get; set; }

        [ForeignKey("SeatClass")]
        public int SeatClassId { get; set; }
        public virtual SeatClass SeatClass { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; } // Giá vé thực trả tại thời điểm mua
    }
}
