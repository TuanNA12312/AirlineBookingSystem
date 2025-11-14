using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class BookingRequestDto
    {
        public int UserId { get; set; }
        public int FlightId { get; set; }
        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
        public int SeatClassId { get; set; }
    }
}
