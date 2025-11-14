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
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterRequestDto request)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null) return BadRequest("Email đã tồn tại");

            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = request.Password
            };
            await _userRepo.AddAsync(newUser);
            return Ok("Đăng ký thành công");
        }

        // POST: /api/users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Login(LoginRequestDto request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null) return Unauthorized("Email hoặc mật khẩu không đúng");

            var isPasswordValid = await _userRepo.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return Unauthorized("Email hoặc mật khẩu không đúng");

            // Đăng nhập thành công, TẠO TOKEN
            var token = _tokenService.CreateToken(user);

            // Trả về Token và thông tin user (trừ password)
            user.PasswordHash = string.Empty;
            return Ok(new LoginResponseDto
            {
                Token = token,
                UserInfo = user
            });
        }

        // GET: /api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            foreach (var user in users) { user.PasswordHash = string.Empty; } // Xóa hash
            return Ok(users);
        }

        // GET: /api/users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();
            user.PasswordHash = string.Empty;
            return Ok(user);
        }

        // DELETE: /api/users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _userRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
