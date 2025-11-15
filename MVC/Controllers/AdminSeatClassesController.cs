using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class AdminSeatClassesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminSeatClassesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        // GET: /Admin/AdminSeatClasses
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = GetClientWithAuth();
            var response = await client.GetAsync("api/SeatClasses");

            if (response.IsSuccessStatusCode)
            {
                var classes = await response.Content.ReadFromJsonAsync<IEnumerable<SeatClass>>(_jsonOptions);
                return View(classes ?? new List<SeatClass>());
            }

            TempData["ErrorMessage"] = "Không thể tải danh sách hạng ghế.";
            return View(new List<SeatClass>());
        }

        // GET, POST, EDIT, DELETE logic tương tự AdminAirlinesController, sử dụng SeatClassId (int) làm khóa chính.

        // GET: /Admin/AdminSeatClasses/Create
        [HttpGet]
        public IActionResult Create() => View(new SeatClass());

        // POST: /Admin/AdminSeatClasses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeatClass model)
        {
            if (ModelState.IsValid)
            {
                var client = GetClientWithAuth();
                var response = await client.PostAsJsonAsync("api/SeatClasses", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm hạng ghế thành công.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = $"Thêm hạng ghế thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            return View(model);
        }

        // GET: /Admin/AdminSeatClasses/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = GetClientWithAuth();
            var seatClass = await client.GetFromJsonAsync<SeatClass>($"api/SeatClasses/{id}");
            if (seatClass == null) return NotFound();
            return View(seatClass);
        }

        // POST: /Admin/AdminSeatClasses/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SeatClass model)
        {
            if (id != model.SeatClassId) return BadRequest();
            if (ModelState.IsValid)
            {
                var client = GetClientWithAuth();
                var response = await client.PutAsJsonAsync($"api/SeatClasses/{id}", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật hạng ghế thành công.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = $"Cập nhật thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            return View(model);
        }

        // POST: /Admin/AdminSeatClasses/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = GetClientWithAuth();
            var response = await client.DeleteAsync($"api/SeatClasses/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa hạng ghế thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Xóa thất bại. {await response.Content.ReadAsStringAsync()}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}