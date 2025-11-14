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

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
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
                    // 1. Lưu Token vào Session (để HttpClient tự động gắn vào)
                    HttpContext.Session.SetString("JWToken", loginResponse.Token);
                    // 2. Lưu thông tin User vào Session (để _Layout hiển thị)
                    HttpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(loginResponse.UserInfo));

                    // 3. Tạo Cookie Authentication (để [Authorize] của MVC hoạt động)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginResponse.UserInfo.FullName),
                        new Claim(ClaimTypes.Email, loginResponse.UserInfo.Email),
                        new Claim("UserId", loginResponse.UserInfo.UserId.ToString()),
                        new Claim(ClaimTypes.Role, loginResponse.UserInfo.IsAdmin ? "Admin" : "User")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // 4. Chuyển hướng
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    if (loginResponse.UserInfo.IsAdmin)
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Kiểm tra Email hoặc Mật khẩu.");
            return View(model);
        }

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

            string apiError = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, apiError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}