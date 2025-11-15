using BusinessObject; // Dùng Entity Airport
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace MVC.Controllers
{
    // Chỉ cho phép Admin truy cập Controller này
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class AdminAirportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminAirportsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7123";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Helper để lấy HttpClient đã gán Token
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

        // GET: /Admin/AdminAirports
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = GetClientWithAuth();
            var response = await client.GetAsync("api/Airports");

            if (response.IsSuccessStatusCode)
            {
                var airports = await response.Content.ReadFromJsonAsync<IEnumerable<Airport>>(_jsonOptions);
                return View(airports ?? new List<Airport>());
            }

            TempData["ErrorMessage"] = "Không thể tải danh sách sân bay.";
            return View(new List<Airport>());
        }

        // GET: /Admin/AdminAirports/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Airport());
        }

        // POST: /Admin/AdminAirports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Airport model)
        {
            if (ModelState.IsValid)
            {
                var client = GetClientWithAuth();

                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Airports", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm sân bay thành công.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = $"Thêm sân bay thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            return View(model);
        }

        // GET: /Admin/AdminAirports/Edit/SGN
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var client = GetClientWithAuth();
            var response = await client.GetAsync($"api/Airports/{id}");

            if (response.IsSuccessStatusCode)
            {
                var airport = await response.Content.ReadFromJsonAsync<Airport>(_jsonOptions);
                if (airport == null) return NotFound();
                return View(airport);
            }

            TempData["ErrorMessage"] = "Không tìm thấy sân bay để sửa.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminAirports/Edit/SGN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Airport model)
        {
            if (id != model.AirportCode) return BadRequest();

            if (ModelState.IsValid)
            {
                var client = GetClientWithAuth();

                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/Airports/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật sân bay thành công.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = $"Cập nhật thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            return View(model);
        }

        // GET: /Admin/AdminAirports/Delete/SGN
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var client = GetClientWithAuth();
            var response = await client.GetAsync($"api/Airports/{id}");

            if (response.IsSuccessStatusCode)
            {
                var airport = await response.Content.ReadFromJsonAsync<Airport>(_jsonOptions);
                if (airport == null) return NotFound();
                return View(airport);
            }

            TempData["ErrorMessage"] = "Không tìm thấy sân bay để xóa.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminAirports/DeleteConfirmed/SGN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = GetClientWithAuth();
            var response = await client.DeleteAsync($"api/Airports/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa sân bay thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Xóa sân bay thất bại: {await response.Content.ReadAsStringAsync()}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}