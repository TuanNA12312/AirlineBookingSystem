using API.Services;
using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

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
                PasswordHash = request.Password // Lưu thẳng (theo yêu cầu)
            };
            await _userRepo.AddAsync(newUser);
            return Ok("Đăng ký thành công");
        }

        // POST: /api/users/login
        [HttpPost("login")]
        [AllowAnonymous] // (Public)
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null) return Unauthorized("Email hoặc mật khẩu không đúng");

            var isPasswordValid = await _userRepo.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return Unauthorized("Email hoặc mật khẩu không đúng");

            var token = _tokenService.CreateToken(user);
            user.PasswordHash = string.Empty;
            return Ok(new LoginResponseDto { Token = token, UserInfo = user });
        }

        // --- PHẦN ADMIN ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            foreach (var user in users) { user.PasswordHash = string.Empty; }
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound(new { Message = $"Không tìm thấy User với ID {id}" });
            user.PasswordHash = string.Empty;
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _userRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
