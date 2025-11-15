using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class AdminFlightPricesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminFlightPricesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7123";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient GetClientWithAuth()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // Helper: Nạp danh sách Flight và SeatClass cho Dropdown
        private async Task LoadDependenciesAsync(HttpClient client, object selectedFlight = null, object selectedSeatClass = null)
        {
            // Lấy danh sách Flight (giả sử có endpoint)
            var flightResponse = await client.GetAsync("api/Flights");
            var flights = await flightResponse.Content.ReadFromJsonAsync<IEnumerable<Flight>>(_jsonOptions);
            ViewBag.FlightId = new SelectList(flights, "FlightId", "FlightNumber", selectedFlight);

            // Lấy danh sách SeatClass
            var classResponse = await client.GetAsync("api/SeatClasses");
            var classes = await classResponse.Content.ReadFromJsonAsync<IEnumerable<SeatClass>>(_jsonOptions);
            ViewBag.SeatClassId = new SelectList(classes, "SeatClassId", "ClassName", selectedSeatClass);
        }

        // GET: /Admin/AdminFlightPrices
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = GetClientWithAuth();
            var response = await client.GetAsync("api/FlightPrices");

            if (response.IsSuccessStatusCode)
            {
                // API cần Include Flight và SeatClass
                var prices = await response.Content.ReadFromJsonAsync<IEnumerable<FlightPrice>>(_jsonOptions);
                return View(prices ?? new List<FlightPrice>());
            }

            TempData["ErrorMessage"] = "Không thể tải danh sách giá vé.";
            return View(new List<FlightPrice>());
        }

        // GET: /Admin/AdminFlightPrices/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = GetClientWithAuth();
            await LoadDependenciesAsync(client);
            return View(new FlightPrice());
        }

        // POST: /Admin/AdminFlightPrices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightPrice model)
        {
            var client = GetClientWithAuth();
            if (ModelState.IsValid)
            {
                var response = await client.PostAsJsonAsync("api/FlightPrices", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm giá vé thành công.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = $"Thêm giá vé thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            await LoadDependenciesAsync(client, model.FlightId, model.SeatClassId);
            return View(model);
        }

        // POST: /Admin/AdminFlightPrices/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = GetClientWithAuth();
            var response = await client.DeleteAsync($"api/FlightPrices/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa giá vé thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Xóa thất bại.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}