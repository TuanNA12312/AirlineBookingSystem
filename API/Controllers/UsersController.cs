using API.Services;
using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public UsersController(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepo = userRepository;
            _tokenService = tokenService;
        }

        // --- HELPER METHOD: Ánh xạ Entity User sang DTO ---
        private UserSessionDto MapUserToDto(User user)
        {
            // Giả định UserDto là kiểu mà LoginResponseDto mong đợi
            return new UserSessionDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsAdmin = user.IsAdmin // Đã kiểm tra IsAdmin tồn tại trong BusinessObject.User.cs
            };
        }

        // POST: /api/users/register
        [HttpPost("register")]
        [AllowAnonymous] // (Public)
        public async Task<ActionResult> Register(RegisterRequestDto request)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null) return BadRequest("Email đã tồn tại");
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = request.Password, // Lưu thẳng (theo yêu cầu)
                IsAdmin = false, // Mặc định không phải Admin
                // PhoneNumber, etc., nếu có trong RegisterRequestDto
            };
            await _userRepo.AddAsync(newUser);
            return Ok("Đăng ký thành công");
        }

        // POST: /api/users/login (ĐÃ SỬA LỖI MAPPING)
        [HttpPost("login")]
        [AllowAnonymous] // (Public)
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null) return Unauthorized("Email hoặc mật khẩu không đúng");

            var isPasswordValid = await _userRepo.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return Unauthorized("Email hoặc mật khẩu không đúng");

            var token = _tokenService.CreateToken(user);

            // LỖI ĐÃ ĐƯỢC SỬA: Ánh xạ User Entity sang DTO
            var userInfoDto = MapUserToDto(user);

            return Ok(new LoginResponseDto { Token = token, UserInfo = userInfoDto });
        }

        // --- PHẦN ADMIN ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userRepo.GetAllAsync();

            // Ánh xạ từng User sang DTO trước khi trả về để ẩn PasswordHash
            var userDtos = users.Select(u => MapUserToDto(u)).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound(new { Message = $"Không tìm thấy User với ID {id}" });

            // Ánh xạ sang DTO
            var userDto = MapUserToDto(user);

            return Ok(userDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _userRepo.DeleteAsync(id);
            return NoContent();
        }

        // --- PHẦN CẬP NHẬT PROFILE (BƯỚC 1) ---

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UserProfileUpdateDto userDto)
        {
            // Kiểm tra phân quyền: User chỉ được sửa profile của chính họ, trừ Admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = await _userRepo.GetByIdAsync(currentUserId);
            if (currentUserId != id && (currentUser == null || !currentUser.IsAdmin))
            {
                return Forbid("Bạn không có quyền sửa thông tin của người dùng khác.");
            }

            var updatedUser = await _userRepo.UpdateUserProfileAsync(id, userDto);

            if (updatedUser == null)
            {
                return BadRequest("Cập nhật thất bại, có thể do Email bị trùng.");
            }

            // Ánh xạ sang DTO trước khi trả về
            var updatedUserDto = MapUserToDto(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            // Kiểm tra phân quyền: User chỉ được đổi mật khẩu của chính họ
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != changePasswordDto.UserId)
            {
                return Forbid("Bạn không có quyền đổi mật khẩu của người dùng khác.");
            }

            var result = await _userRepo.ChangePasswordAsync(changePasswordDto);

            if (!result)
            {
                return BadRequest("Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.");
            }

            return Ok(new { Message = "Đổi mật khẩu thành công." });
        }
    }
}