using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using MVC.Models; // CHỈ DÙNG DTOs TẠI ĐÂY
using System.Net.Http.Headers;

namespace MVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        public ProfileController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = _configuration["ApiBaseUrl"] ?? throw new ArgumentNullException("ApiBaseUrl", "API Base URL must be configured.");
        }

        // Helper để đọc thông tin user VÀ TOKEN từ Session
        private LoginResponseDto? GetUserSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            // Đọc Session Key "UserInfo"
            var userSessionJson = session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userSessionJson)) return null;

            // Deserialize thành LoginResponseDto. PropertyNameCaseInsensitive = true là bắt buộc.
            return JsonSerializer.Deserialize<LoginResponseDto>(userSessionJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // Helper: Thêm Token vào Header cho HttpClient
        private void AddAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // GET: /Profile/Index - Hiển thị form cập nhật hồ sơ
        public IActionResult Index()
        {
            var userSession = GetUserSession();
            if (userSession == null || userSession.UserInfo == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn.";
                return RedirectToAction("Login", "Account");
            }

            // Dùng UserSessionDto để lấy dữ liệu.
            var model = new UserProfileUpdateViewModel
            {
                UserId = userSession.UserInfo.UserId,
                FullName = userSession.UserInfo.FullName,
                Email = userSession.UserInfo.Email,
                PhoneNumber = userSession.UserInfo.PhoneNumber,
            };

            return View(model);
        }

        // POST: /Profile/Index - Xử lý cập nhật Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserProfileUpdateViewModel model)
        {
            var userSession = GetUserSession();

            if (userSession == null || userSession.UserInfo == null || userSession.UserInfo.UserId != model.UserId)
            {
                TempData["ErrorMessage"] = "Lỗi phiên làm việc, vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AddAuthorizationHeader(userSession.Token);

            // Dùng DTO ẩn danh để khớp với API (tránh phụ thuộc vào DataAccess DTO)
            var updateDto = new
            {
                model.FullName,
                model.Email,
                model.PhoneNumber
            };

            var jsonContent = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Gọi API PUT /api/users/{id}
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/users/{model.UserId}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // API trả về BusinessObject.User (được deserialize thành UserSessionDto)
                var updatedUserEntity = JsonSerializer.Deserialize<UserSessionDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Cập nhật lại Session
                if (updatedUserEntity != null)
                {
                    // Cập nhật trường UserInfo trong LoginResponseDto (Session object)
                    userSession.UserInfo = updatedUserEntity;
                    // Ghi đè Session Key "UserInfo"
                    _httpContextAccessor.HttpContext?.Session.SetString("UserInfo", JsonSerializer.Serialize(userSession));
                }

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật hồ sơ thất bại. Vui lòng kiểm tra Email (có thể đã trùng).";
                return View(model);
            }
        }

        // ... (Giữ nguyên action ChangePassword đã cung cấp ở bước trước)
        public IActionResult ChangePassword()
        {
            var userSession = GetUserSession();
            if (userSession == null || userSession.UserInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Khởi tạo ViewModel với UserId từ Session
            var model = new ChangePasswordViewModel { UserId = userSession.UserInfo.UserId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userSession = GetUserSession();
            if (userSession == null || userSession.UserInfo == null || userSession.UserInfo.UserId != model.UserId)
            {
                TempData["ErrorMessage"] = "Lỗi phiên làm việc, vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AddAuthorizationHeader(userSession.Token);

            var changePasswordDto = new
            {
                model.UserId,
                model.OldPassword,
                model.NewPassword
            };

            var jsonContent = JsonSerializer.Serialize(changePasswordDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/users/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
                _httpContextAccessor.HttpContext?.Session.Clear();
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["ErrorMessage"] = "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.";
                ModelState.AddModelError(string.Empty, "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.");
                return View(model);
            }
        }
    }
}