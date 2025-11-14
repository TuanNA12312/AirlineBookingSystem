using BusinessObject;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class SearchViewModel
    {
        public SelectList? Airports { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn điểm đi")]
        public string FromAirport { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng chọn điểm đến")]
        public string ToAirport { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng chọn ngày đi")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today;
        public IEnumerable<Flight>? SearchResults { get; set; }
    }
}
