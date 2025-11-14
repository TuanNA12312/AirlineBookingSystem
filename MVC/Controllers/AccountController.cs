using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Text.Json;
using System.Text;
using BusinessObject;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // --- HÀM 1: TRANG ĐĂNG NHẬP (GET) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- HÀM 2: XỬ LÝ ĐĂNG NHẬP (POST) ---
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            // Gọi API POST /api/users/login
            var response = await client.PostAsync("/api/users/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // 1. Lưu Token vào Session
                    HttpContext.Session.SetString("JWToken", loginResponse.Token);

                    // 2. Lưu thông tin User vào Session
                    string userJson = JsonSerializer.Serialize(loginResponse.UserInfo);
                    HttpContext.Session.SetString("UserInfo", userJson);

                    // 3. Chuyển hướng về Trang chủ
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng kiểm tra lại Email hoặc Mật khẩu.");
            return View(model);
        }

        // --- HÀM 3: ĐĂNG XUẤT ---
        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa toàn bộ Session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}