using BusinessObject;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class BookingRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        public int SeatClassId { get; set; }

        [Required]
        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
    }
}
