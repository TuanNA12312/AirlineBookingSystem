using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Text.Json;
using System.Net.Http.Headers; // Thêm thư viện này
using System.Net.Http.Json;   // Thêm thư viện này

namespace MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // BỔ SUNG DEPENDENCY QUAN TRỌNG
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        
        // CẬP NHẬT CONSTRUCTOR
        public AdminController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            // Lấy Base URL từ config
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7123"; 
        }

        // --- HELPER METHOD ---
        // Phương thức này tạo HttpClient và gán Token cho nó
        private HttpClient GetClientWithAuth()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiBaseUrl); 
            
            // Lấy token từ Session
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // GET: /Admin/Index (Hoạt động như Dashboard)
        public async Task<IActionResult> Index()
        {
            // SỬ DỤNG CLIENT ĐÃ GÁN TOKEN
            var client = GetClientWithAuth(); 
            
            // API Endpoint Report
            var response = await client.GetAsync("api/reports/dashboard-stats");

            if (response.IsSuccessStatusCode)
            {
                // Dùng ReadFromJsonAsync
                var model = await response.Content.ReadFromJsonAsync<AdminDashboardViewModel>(_jsonOptions);
                return View(model);
            }

            // Xử lý lỗi Unauthorized/Forbidden
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Nếu không có quyền, xóa session và chuyển hướng về Login
                _httpContextAccessor.HttpContext?.Session.Clear();
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc không có quyền truy cập.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ErrorMessage = $"Lỗi tải báo cáo: {response.ReasonPhrase}";
            return View(new AdminDashboardViewModel());
        }
    }
}