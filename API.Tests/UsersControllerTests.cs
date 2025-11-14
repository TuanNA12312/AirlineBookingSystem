using API.Controllers;
using API.Services;
using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly UsersController _controller;

        // Constructor: Chạy trước mỗi [Fact] (mỗi test)
        public UsersControllerTests()
        {
            // 1. Giả lập các DI (Dependency)
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();

            // 2. "Tiêm" (Inject) các DI giả vào Controller
            _controller = new UsersController(_mockUserRepo.Object, _mockTokenService.Object);
        }

        // --- Test 1: Đăng ký thành công ---
        [Fact]
        public async Task Register_Returns_Ok_When_Email_Is_New()
        {
            // --- ARRANGE (SẮP ĐẶT) ---
            var request = new RegisterRequestDto { Email = "new@gmail.com", Password = "123", FullName = "Test" };

            // "Dạy" cho Repo giả: "Khi GetUserByEmailAsync được gọi VỚI BẤT KỲ EMAIL NÀO,
            // hãy trả về NULL (email chưa tồn tại)"
            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync((User)null);

            // --- ACT (HÀNH ĐỘNG) ---
            var result = await _controller.Register(request);

            // --- ASSERT (KHẲNG ĐỊNH) ---
            // 1. Khẳng định: Kết quả trả về phải là "Ok"
            Assert.IsType<OkObjectResult>(result);

            // 2. Khẳng định: Hàm AddAsync() PHẢI được gọi ĐÚNG 1 LẦN
            _mockUserRepo.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        // --- Test 2: Đăng ký thất bại (Email đã tồn tại) ---
        [Fact]
        public async Task Register_Returns_BadRequest_When_Email_Exists()
        {
            // --- ARRANGE (SẮP ĐẶT) ---
            var request = new RegisterRequestDto { Email = "old@gmail.com", Password = "123", FullName = "Test" };

            // "Dạy" cho Repo giả: "Khi GetUserByEmailAsync được gọi,
            // hãy trả về một User (email đã tồn tại)"
            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync(new User()); // Trả về 1 User (không null)

            // --- ACT (HÀNH ĐỘNG) ---
            var result = await _controller.Register(request);

            // --- ASSERT (KHẲNG ĐỊNH) ---
            // 1. Khẳng định: Kết quả trả về phải là "BadRequest"
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // 2. Khẳng định: Thông báo lỗi phải là "Email đã tồn tại"
            Assert.Equal("Email đã tồn tại", badRequestResult.Value);

            // 3. Khẳng định: Hàm AddAsync() KHÔNG BAO GIỜ được gọi
            _mockUserRepo.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }

        // --- Test 3: Đăng nhập thành công ---
        [Fact]
        public async Task Login_Returns_OkWithToken_When_Credentials_Are_Valid()
        {
            // --- ARRANGE (SẮP ĐẶT) ---
            var request = new LoginRequestDto { Email = "admin@gmail.com", Password = "admin123" };
            var fakeUser = new User { UserId = 1, Email = "admin@gmail.com" };
            var fakeToken = "fake.jwt.token";

            // 1. Dạy Repo: Khi GetUserByEmailAsync, trả về User
            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync(fakeUser);

            // 2. Dạy Repo: Khi CheckPasswordAsync, trả về TRUE
            _mockUserRepo.Setup(repo => repo.CheckPasswordAsync(fakeUser, request.Password))
                         .ReturnsAsync(true);

            // 3. Dạy Service: Khi CreateToken, trả về "fakeToken"
            _mockTokenService.Setup(service => service.CreateToken(fakeUser))
                             .Returns(fakeToken);

            // --- ACT (HÀNH ĐỘNG) ---
            var result = await _controller.Login(request);

            // --- ASSERT (KHẲNG ĐỊNH) ---
            // 1. Khẳng định: Kết quả là OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // 2. Khẳng định: Dữ liệu trả về là LoginResponseDto
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);

            // 3. Khẳng định: Token phải chính xác
            Assert.Equal(fakeToken, responseDto.Token);
            Assert.Equal(1, responseDto.UserInfo.UserId);
        }

        // --- Test 4: Đăng nhập thất bại (Sai mật khẩu) ---
        [Fact]
        public async Task Login_Returns_Unauthorized_When_Password_Is_Wrong()
        {
            // --- ARRANGE (SẮP ĐẶT) ---
            var request = new LoginRequestDto { Email = "user@gmail.com", Password = "sai_password" };
            var fakeUser = new User { UserId = 2, Email = "user@gmail.com" };

            // 1. Dạy Repo: Tìm thấy User
            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync(fakeUser);

            // 2. Dạy Repo: Mật khẩu SAI
            _mockUserRepo.Setup(repo => repo.CheckPasswordAsync(fakeUser, request.Password))
                         .ReturnsAsync(false); // <-- Quan trọng

            // --- ACT (HÀNH ĐỘNG) ---
            var result = await _controller.Login(request);

            // --- ASSERT (KHẲNG ĐỊNH) ---
            // 1. Khẳng định: Kết quả là UnauthorizedObjectResult (Lỗi 401)
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Email hoặc mật khẩu không đúng", unauthorizedResult.Value);

            // 2. Khẳng định: Hàm CreateToken() KHÔNG BAO GIỜ được gọi
            _mockTokenService.Verify(service => service.CreateToken(It.IsAny<User>()), Times.Never);
        }
    }
}
