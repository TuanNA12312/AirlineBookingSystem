using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class AdminBookingsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminBookingsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        // GET: /Admin/AdminBookings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = GetClientWithAuth();
            // Gọi API để lấy TẤT CẢ bookings (giả sử API/BookingsController có endpoint này)
            var response = await client.GetAsync("api/Bookings");

            if (response.IsSuccessStatusCode)
            {
                // API trả về List<Booking>
                var bookings = await response.Content.ReadFromJsonAsync<IEnumerable<Booking>>(_jsonOptions);
                return View(bookings ?? new List<Booking>());
            }

            TempData["ErrorMessage"] = "Không thể tải danh sách đơn đặt vé.";
            return View(new List<Booking>());
        }

        // GET: /Admin/AdminBookings/Details/5 (Dùng lại View Details của User)
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var client = GetClientWithAuth();
            // API trả về Booking (Entity) bao gồm Flight và Tickets
            var booking = await client.GetFromJsonAsync<Booking>($"api/bookings/{id}");

            if (booking == null) return NotFound();

            // Dùng View Details đã tạo ở Bước 2
            return View("~/Views/Booking/Details.cshtml", booking);
        }

        // POST: /Admin/AdminBookings/Cancel/5 (Hủy booking thủ công)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var client = GetClientWithAuth();
            var response = await client.PostAsync($"api/bookings/{id}/cancel", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Hủy đơn đặt chỗ #{id} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Hủy đơn đặt chỗ #{id} thất bại: {await response.Content.ReadAsStringAsync()}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminBookings/Confirm/5 (Xác nhận Booking thủ công)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var client = GetClientWithAuth();
            // API cần có endpoint này để admin xác nhận thủ công
            var response = await client.PostAsync($"api/bookings/{id}/confirm", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Xác nhận đơn đặt chỗ #{id} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Xác nhận đơn đặt chỗ #{id} thất bại: {await response.Content.ReadAsStringAsync()}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}