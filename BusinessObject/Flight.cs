using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [ForeignKey("DepartureAirport")]
        public string DepartureAirportCode { get; set; }
        public virtual Airport DepartureAirport { get; set; }

        [Required]
        [ForeignKey("ArrivalAirport")]
        public string ArrivalAirportCode { get; set; }
        public virtual Airport ArrivalAirport { get; set; }

        [Required]
        [ForeignKey("Airline")]
        public string AirlineCode { get; set; }
        public virtual Airline Airline { get; set; }

        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public virtual ICollection<FlightPrice> Prices { get; set; } = new List<FlightPrice>();
    }
}
