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
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: /Booking/Create?flightId=1
        [HttpGet]
        public async Task<IActionResult> Create(int flightId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Gọi API /api/flights/{id}
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

            // (Logic lọc hạng ghế)
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
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            if (!ModelState.IsValid)
            {
                // (Nạp lại dữ liệu nếu Form lỗi)
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

            // (Lỗi 1 đã được sửa bằng cách thêm file AllViewModels.cs)
            var bookingRequest = new BookingRequestDto
            {
                UserId = int.Parse(userId),
                FlightId = model.FlightId,
                SeatClassId = model.SeatClassId,
                Passengers = new List<Passenger> { passenger }
            };

            // (Lỗi 2 đã được sửa bằng cách thêm 'using System.Text;')
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(bookingRequest),
                Encoding.UTF8,
                "application/json" // <-- Lỗi đỏ sẽ hết
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

        // GET: /Booking/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue("UserId");
            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.GetAsync($"/api/bookings/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bookings = JsonSerializer.Deserialize<List<Booking>>(content, _jsonOptions);
                return View(bookings);
            }

            TempData["ErrorMessage"] = "Không thể tải lịch sử đặt vé.";
            return View(new List<Booking>());
        }
    }
}