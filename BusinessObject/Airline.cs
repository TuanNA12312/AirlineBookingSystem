using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Airline
    {
        [Key]
        [StringLength(2)]
        public string AirlineCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AirlineName { get; set; } = string.Empty;

        public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}
