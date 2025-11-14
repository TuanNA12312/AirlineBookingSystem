using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Text.Json;

namespace MVC.Controllers
{
    [Area("Admin")] // <-- Khai báo Area
    [Authorize(Roles = "Admin")] // <-- BẢO MẬT: CHỈ ADMIN
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // GET: /Admin/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API /api/reports/dashboard-stats
            var response = await client.GetAsync("/api/reports/dashboard-stats");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<AdminDashboardViewModel>(content, _jsonOptions);
                return View(model);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            ViewBag.ErrorMessage = "Lỗi tải báo cáo";
            return View(new AdminDashboardViewModel());
        }
    }
}
