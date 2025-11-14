using BusinessObject;
using Microsoft.AspNetCore.Authorization; // <-- BẢO MẬT
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    [Authorize] // <-- BẮT BUỘC ĐĂNG NHẬP
    public class BookingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- HÀM 1: TRANG CHỌN VÉ (ĐIỀN HÀNH KHÁCH) ---
        // GET: /Booking/Create?flightId=1
        [HttpGet]
        public async Task<IActionResult> Create(int flightId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Gọi API /api/flights/{id} (đã sửa thành [AllowAnonymous])
            var flightResponse = await client.GetAsync($"/api/flights/{flightId}");

            // 2. Gọi API /api/seatclasses (public)
            var seatClassResponse = await client.GetAsync("/api/seatclasses");

            if (!flightResponse.IsSuccessStatusCode || !seatClassResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến bay hoặc hạng ghế.";
                return RedirectToAction("Index", "Home");
            }

            var flightContent = await flightResponse.Content.ReadAsStringAsync();
            var flight = JsonSerializer.Deserialize<Flight>(flightContent, _jsonOptions);

            var seatClassContent = await seatClassResponse.Content.ReadAsStringAsync();
            var seatClasses = JsonSerializer.Deserialize<IEnumerable<SeatClass>>(seatClassContent, _jsonOptions);

            // Tạo ViewModel
            var model = new BookingViewModel
            {
                FlightId = flight.FlightId,
                FlightDetails = flight,
                SeatClasses = new SelectList(seatClasses, "SeatClassId", "ClassName"),
                PassengerDob = DateTime.Today.AddYears(-20) // Gợi ý ngày sinh
            };

            return View(model);
        }

        // --- HÀM 2: XỬ LÝ ĐẶT VÉ ---
        // POST: /Booking/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            // Lấy UserId từ Cookie/Session
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Lỗi (nên không xảy ra)
            }

            // 1. Tạo đối tượng Passenger (từ Form)
            var passenger = new Passenger
            {
                FullName = model.PassengerName,
                DateOfBirth = model.PassengerDob
                // (PassportNumber có thể thêm vào form)
            };

            // 2. Tạo Request DTO để gửi cho API
            var bookingRequest = new BookingRequestDto
            {
                UserId = int.Parse(userId),
                FlightId = model.FlightId,
                SeatClassId = model.SeatClassId,
                Passengers = new List<Passenger> { passenger }
            };

            // 3. Gọi API /api/bookings (API này [Authorize])
            // (HttpClient đã được cấu hình tự động gắn Token)
            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = new StringContent(JsonSerializer.Serialize(bookingRequest), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/bookings", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đặt vé thành công!";
                return RedirectToAction("History");
            }

            // Nếu lỗi (Hết vé, lỗi 400...)
            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Lỗi: {errorContent}";
            return RedirectToAction("Index", "Home");
        }

        // --- HÀM 3: XEM LỊCH SỬ ĐẶT VÉ ---
        // GET: /Booking/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue("UserId");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API /api/bookings/user/{id} (API này [Authorize])
            var response = await client.GetAsync($"/api/bookings/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bookings = JsonSerializer.Deserialize<List<Booking>>(content, _jsonOptions);
                return View(bookings);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "Không thể tải lịch sử đặt vé.";
            return View(new List<Booking>());
        }
    }
}