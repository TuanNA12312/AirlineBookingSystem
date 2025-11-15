using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations; // Cần cho ViewModel

// ViewModel đơn giản để Admin chỉ sửa những trường cần thiết
public class UserEditModel
{
    public int UserId { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    // Chỉ Admin mới được sửa Role
    [Required]
    public string Role { get; set; }
    public bool IsActive { get; set; } = true;
}

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class AdminUsersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminUsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        // GET: /Admin/AdminUsers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = GetClientWithAuth();
            var response = await client.GetAsync("api/Users");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>(_jsonOptions);
                return View(users ?? new List<User>());
            }

            // Xử lý lỗi
            TempData["ErrorMessage"] = "Không thể tải danh sách người dùng.";
            return View(new List<User>());
        }

        // GET: /Admin/AdminUsers/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = GetClientWithAuth();
            var response = await client.GetAsync($"api/Users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>(_jsonOptions);
                if (user == null) return NotFound();

                // Map User entity sang ViewModel để chỉnh sửa
                var model = new UserEditModel
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                };
                return View(model);
            }

            TempData["ErrorMessage"] = "Không tìm thấy người dùng để sửa.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AdminUsers/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditModel model)
        {
            if (id != model.UserId) return BadRequest();

            if (ModelState.IsValid)
            {
                var client = GetClientWithAuth();

                // API có thể cần nhận User entity đầy đủ hoặc một DTO tương đương. 
                // Ta sẽ gửi ViewModel và giả định API chấp nhận các trường này.
                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/Users/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = $"Cập nhật người dùng thất bại: {await response.Content.ReadAsStringAsync()}";
            }
            return View(model);
        }

        // POST: /Admin/AdminUsers/Delete/1 (Hành động xóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetClientWithAuth();
            var response = await client.DeleteAsync($"api/Users/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa người dùng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Xóa người dùng thất bại: {await response.Content.ReadAsStringAsync()}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}