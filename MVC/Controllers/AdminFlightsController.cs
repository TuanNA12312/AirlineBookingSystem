using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFlightsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminFlightsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: /AdminFlights
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            // (HttpClient đã tự gắn Token Admin)
            var response = await client.GetAsync("/api/flights");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var flights = JsonSerializer.Deserialize<List<Flight>>(content, _jsonOptions);
                return View(flights);
            }
            return View(new List<Flight>());
        }

        // GET: /AdminFlights/Create
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            await LoadDropdowns(client);

            return View();
        }

        // POST: /AdminFlights/Create
        [HttpPost]
        public async Task<IActionResult> Create(Flight flight)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Kiểm tra lỗi (Validate)
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, nạp lại Dropdown
                await LoadDropdowns(client);
                return View(flight);
            }

            // Gán logic
            flight.AvailableSeats = flight.TotalSeats;

            var jsonContent = new StringContent(JsonSerializer.Serialize(flight), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/flights", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Tạo chuyến bay thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu API báo lỗi
            await LoadDropdowns(client);
            ModelState.AddModelError(string.Empty, "Lỗi khi tạo chuyến bay.");
            return View(flight);
        }
        private async Task LoadDropdowns(HttpClient client)
        {
            // (Chạy song song 2 request API cho nhanh)
            var airportsTask = client.GetAsync("/api/airports");
            var airlinesTask = client.GetAsync("/api/airlines");

            await Task.WhenAll(airportsTask, airlinesTask);

            // Xử lý kết quả
            var airportsContent = await airportsTask.Result.Content.ReadAsStringAsync();
            var airlinesContent = await airlinesTask.Result.Content.ReadAsStringAsync();

            var airports = JsonSerializer.Deserialize<IEnumerable<Airport>>(airportsContent, _jsonOptions);
            var airlines = JsonSerializer.Deserialize<IEnumerable<Airline>>(airlinesContent, _jsonOptions);

            // Gán vào ViewBag để View (Form) có thể đọc
            ViewBag.Airports = new SelectList(airports, "AirportCode", "AirportName");
            ViewBag.Airlines = new SelectList(airlines, "AirlineCode", "AirlineName");
        }
    }
}