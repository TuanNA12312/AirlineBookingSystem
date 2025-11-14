using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Airport
    {
        [Key]
        [StringLength(3)]
        public string AirportCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AirportName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
    }
}
