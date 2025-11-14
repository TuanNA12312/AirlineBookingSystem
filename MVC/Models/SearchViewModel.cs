using BusinessObject;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    /// <summary>
    /// ViewModel: Dữ liệu mà View (Trang Home/Index) cần
    /// </summary>
    public class SearchViewModel
    {
        // 1. Dữ liệu cho các Dropdown List
        public SelectList? Airports { get; set; }

        // 2. Dữ liệu người dùng nhập vào Form
        [Required(ErrorMessage = "Vui lòng chọn điểm đi")]
        public string FromAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn điểm đến")]
        public string ToAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày đi")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today;

        // 3. Dữ liệu trả về (Kết quả tìm kiếm)
        public IEnumerable<Flight>? SearchResults { get; set; }
    }
}
