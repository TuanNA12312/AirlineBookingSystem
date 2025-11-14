using BusinessObject;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- HÀM 1: TRANG ĐĂNG NHẬP (GET) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- HÀM 2: XỬ LÝ ĐĂNG NHẬP (POST) - ĐÃ CẬP NHẬT ---
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/users/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // 1. Lưu Token vào Session (để HttpClient tự động gắn)
                    HttpContext.Session.SetString("JWToken", loginResponse.Token);
                    // 2. Lưu thông tin User vào Session (để _Layout hiển thị)
                    HttpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(loginResponse.UserInfo));

                    // 3. (CODE MỚI) Tạo Cookie Authentication (để [Authorize] của MVC hoạt động)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginResponse.UserInfo.FullName),
                        new Claim(ClaimTypes.Email, loginResponse.UserInfo.Email),
                        new Claim("UserId", loginResponse.UserInfo.UserId.ToString()),
                        new Claim(ClaimTypes.Role, loginResponse.UserInfo.IsAdmin ? "Admin" : "User")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true // (Ghi nhớ đăng nhập)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // 4. Chuyển hướng
                    if (loginResponse.UserInfo.IsAdmin)
                    {
                        // (Đổi route, không dùng Area)
                        return RedirectToAction("Index", "Dashboard");
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng kiểm tra lại Email hoặc Mật khẩu.");
            return View(model);
        }

        // --- HÀM 3: ĐĂNG KÝ (GET/POST) ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/users/register", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Email đã tồn tại hoặc lỗi.");
            return View(model);
        }

        // --- HÀM 4: ĐĂNG XUẤT (CẬP NHẬT) ---
        [HttpPost] // (Dùng POST)
        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Xóa Session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // --- HÀM 5: TRANG CẤM TRUY CẬP (AccessDenied) ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}