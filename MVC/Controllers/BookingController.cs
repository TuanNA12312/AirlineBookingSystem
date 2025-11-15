using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Models;
using System.Security.Claims; // Cần thiết nếu vẫn muốn dùng FindFirstValue (nhưng sẽ sửa)
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers; // Cần thiết cho Authorization
using System.Net.Http.Json; // Cần thiết cho ReadFromJsonAsync

namespace MVC.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        // BỔ SUNG 2 DEPENDENCY QUAN TRỌNG
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;


        // CẬP NHẬT CONSTRUCTOR
        public BookingController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? throw new ArgumentNullException("ApiBaseUrl", "API Base URL must be configured.");
        }

        // --- HELPER METHODS (Dùng lại logic đã thống nhất) ---

        // Lấy Session (chứa Token và UserInfo)
        private LoginResponseDto? GetUserSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;
            var userSessionJson = session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userSessionJson)) return null;
            return JsonSerializer.Deserialize<LoginResponseDto>(userSessionJson, _jsonOptions);
        }

        // Tạo Client và gán Token
        private HttpClient GetClientWithAuth()
        {
            // Sử dụng client mặc định và tự gán BaseAddress/Header
            var client = _httpClientFactory.CreateClient();
            var userSession = GetUserSession();

            if (userSession == null || string.IsNullOrEmpty(userSession.Token))
            {
                // Không có token, sẽ fail auth ở API
                return client;
            }

            client.BaseAddress = new Uri(_apiBaseUrl); // Gán Base Address

            // Thêm Authorization Header
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userSession.Token);

            return client;
        }

        // --- CÁC HÀM CREATE (Sửa để dùng Auth Token) ---

        // GET: /Booking/Create?flightId=1
        [HttpGet]
        public async Task<IActionResult> Create(int flightId)
        {
            // Thay vì client.CreateClient("ApiClient"), ta dùng client mặc định
            var client = GetClientWithAuth();

            // 1. Gọi API /api/flights/{id}
            var flightResponse = await client.GetAsync($"/api/flights/{flightId}");

            // 2. Gọi API /api/seatclasses (public)
            var seatClassResponse = await client.GetAsync("/api/seatclasses");

            // ... (Phần logic còn lại giữ nguyên) ...

            if (!flightResponse.IsSuccessStatusCode || !seatClassResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến bay hoặc hạng ghế.";
                return RedirectToAction("Index", "Home");
            }

            // ... (Deserialize logic giữ nguyên) ...

            var flightContent = await flightResponse.Content.ReadAsStringAsync();
            var flight = JsonSerializer.Deserialize<Flight>(flightContent, _jsonOptions);

            var seatClassContent = await seatClassResponse.Content.ReadAsStringAsync();
            var seatClasses = JsonSerializer.Deserialize<IEnumerable<SeatClass>>(seatClassContent, _jsonOptions);

            var availableSeatClasses = seatClasses.Where(s =>
                flight.Prices.Any(p => p.SeatClassId == s.SeatClassId));

            // Tạo ViewModel
            var model = new BookingViewModel
            {
                FlightId = flight.FlightId,
                FlightDetails = flight,
                SeatClasses = new SelectList(availableSeatClasses, "SeatClassId", "ClassName"),
                PassengerDob = DateTime.Today.AddYears(-20)
            };

            return View(model);
        }

        // POST: /Booking/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            // SỬA: Lấy UserId từ Session thay vì Claims
            var userSession = GetUserSession();
            if (userSession == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = userSession.UserInfo.UserId;

            var client = GetClientWithAuth();

            if (!ModelState.IsValid)
            {
                // ... (Logic nạp lại dữ liệu nếu Form lỗi giữ nguyên) ...
                var flightResponse = await client.GetAsync($"/api/flights/{model.FlightId}");
                var seatClassResponse = await client.GetAsync("/api/seatclasses");
                var flightContent = await flightResponse.Content.ReadAsStringAsync();
                var flight = JsonSerializer.Deserialize<Flight>(flightContent, _jsonOptions);
                var seatClassContent = await seatClassResponse.Content.ReadAsStringAsync();
                var seatClasses = JsonSerializer.Deserialize<IEnumerable<SeatClass>>(seatClassContent, _jsonOptions);
                var availableSeatClasses = seatClasses.Where(s => flight.Prices.Any(p => p.SeatClassId == s.SeatClassId));

                model.FlightDetails = flight;
                model.SeatClasses = new SelectList(availableSeatClasses, "SeatClassId", "ClassName");
                return View(model);
            }

            var passenger = new Passenger
            {
                FullName = model.PassengerName,
                DateOfBirth = model.PassengerDob
            };

            var bookingRequest = new BookingRequestDto
            {
                // SỬA: Dùng userId lấy từ Session
                UserId = userId,
                FlightId = model.FlightId,
                SeatClassId = model.SeatClassId,
                Passengers = new List<Passenger> { passenger }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(bookingRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("/api/bookings", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đặt vé thành công!";
                return RedirectToAction("History");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Lỗi khi đặt vé: {errorContent}";
            return RedirectToAction("Index", "Home");
        }

        // --- CÁC HÀM QUẢN LÝ SAU ĐẶT VÉ (BƯỚC 2) ---

        // GET: /Booking/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userSession = GetUserSession();
            if (userSession == null) return RedirectToAction("Login", "Account");

            int userId = userSession.UserInfo.UserId;
            var client = GetClientWithAuth();

            // API endpoint cũ của bạn: /api/bookings/user/{userId}
            var response = await client.GetAsync($"/api/bookings/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // API trả về List<Booking>
                var bookings = JsonSerializer.Deserialize<List<Booking>>(content, _jsonOptions);
                return View(bookings ?? new List<Booking>());
            }

            TempData["ErrorMessage"] = "Không thể tải lịch sử đặt vé.";
            return View(new List<Booking>());
        }

        // GET: /Booking/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userSession = GetUserSession();
            if (userSession == null) return RedirectToAction("Login", "Account");

            var client = GetClientWithAuth();

            // API endpoint: /api/bookings/{id}
            var response = await client.GetAsync($"/api/bookings/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chi tiết đơn đặt chỗ.";
                return RedirectToAction(nameof(History));
            }

            // API trả về Booking Entity kèm theo các Include
            var booking = await response.Content.ReadFromJsonAsync<Booking>(_jsonOptions);

            if (booking == null) return NotFound();

            // Kỹ thuật: Bảo vệ dữ liệu ở MVC
            if (booking.UserId != userSession.UserInfo.UserId && userSession.UserInfo.IsAdmin != true)
            {
                return Forbid();
            }

            return View(booking);
        }

        // POST: /Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userSession = GetUserSession();
            if (userSession == null) return RedirectToAction("Login", "Account");

            var client = GetClientWithAuth();

            // API endpoint: /api/bookings/{id}/cancel
            var response = await client.PostAsync($"/api/bookings/{id}/cancel", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Hủy đơn đặt chỗ thành công.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Hủy đơn đặt chỗ thất bại. Chi tiết: {errorContent}";
            }

            return RedirectToAction(nameof(History));
        }
    }
}