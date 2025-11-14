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

        public async Task<IActionResult> Index()
        {
            var model = new SearchViewModel { DepartureDate = DateTime.Today.AddDays(1) };
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/airports"); // (API đã chuẩn hóa)

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

        [HttpPost]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var airportResponse = await client.GetAsync("/api/airports");
            if (airportResponse.IsSuccessStatusCode)
            {
                string content = await airportResponse.Content.ReadAsStringAsync();
                var airports = JsonSerializer.Deserialize<IEnumerable<Airport>>(content, _jsonOptions);
                model.Airports = new SelectList(airports, "AirportCode", "AirportName");
            }

            if (model.FromAirport == model.ToAirport)
            {
                ModelState.AddModelError("ToAirport", "Điểm đi và điểm đến không được trùng nhau.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string searchUrl = $"/api/flights/search?from={model.FromAirport}&to={model.ToAirport}&date={model.DepartureDate:yyyy-MM-dd}";
            var searchResponse = await client.GetAsync(searchUrl);

            if (searchResponse.IsSuccessStatusCode)
            {
                string content = await searchResponse.Content.ReadAsStringAsync();
                model.SearchResults = JsonSerializer.Deserialize<IEnumerable<Flight>>(content, _jsonOptions);
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi khi tìm kiếm chuyến bay.";
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