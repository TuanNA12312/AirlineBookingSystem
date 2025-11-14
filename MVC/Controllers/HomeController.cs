using BusinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- HÀM 1: HIỂN THỊ TRANG CHỦ (FORM TÌM KIẾM) ---
        public async Task<IActionResult> Index()
        {
            var model = new SearchViewModel { DepartureDate = DateTime.Today.AddDays(1) };

            // Gọi API /api/airports (public)
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/airports");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var airports = JsonSerializer.Deserialize<IEnumerable<Airport>>(content, _jsonOptions);
                model.Airports = new SelectList(airports, "AirportCode", "AirportName");
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi: Không thể tải danh sách sân bay.";
            }
            return View(model);
        }

        // --- HÀM 2: XỬ LÝ KHI USER NHẤN NÚT "TÌM KIẾM" ---
        [HttpPost]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Load lại Sân bay
            var airportResponse = await client.GetAsync("/api/airports");
            if (airportResponse.IsSuccessStatusCode)
            {
                string content = await airportResponse.Content.ReadAsStringAsync();
                var airports = JsonSerializer.Deserialize<IEnumerable<Airport>>(content, _jsonOptions);
                model.Airports = new SelectList(airports, "AirportCode", "AirportName");
            }

            // 2. Validate form
            if (model.FromAirport == model.ToAirport)
            {
                ModelState.AddModelError("ToAirport", "Điểm đi và điểm đến không được trùng nhau.");
            }
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về View với các lỗi
            }

            // 3. Gọi API /api/flights/search
            string searchUrl = $"/api/flights/search?from={model.FromAirport}&to={model.ToAirport}&date={model.DepartureDate:yyyy-MM-dd}";
            var searchResponse = await client.GetAsync(searchUrl);

            if (searchResponse.IsSuccessStatusCode)
            {
                string content = await searchResponse.Content.ReadAsStringAsync();
                model.SearchResults = JsonSerializer.Deserialize<IEnumerable<Flight>>(content, _jsonOptions);
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi khi tìm kiếm chuyến bay. Vui lòng thử lại.";
                model.SearchResults = new List<Flight>();
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}