using API.Controllers;
using BusinessObject;
using Microsoft.AspNetCore.Mvc;
using Moq; // Dùng để "giả lập" Repository
using Repositories;
using Repositories.Interfaces;
using Xunit; // Dùng để viết Test

namespace API.Tests
{
    public class FlightsControllerTests
    {
        private readonly Mock<IFlightRepository> _mockFlightRepo;
        private readonly FlightsController _controller;

        public FlightsControllerTests()
        {
            _mockFlightRepo = new Mock<IFlightRepository>();
            _controller = new FlightsController(_mockFlightRepo.Object);
        }

        // --- Test 1: Tìm kiếm thấy kết quả ---
        [Fact]
        public async Task SearchFlights_Returns_Ok_With_Flights()
        {
            // --- ARRANGE ---
            var fakeFlights = new List<Flight> { new Flight { FlightId = 1 } };
            var from = "SGN";
            var to = "HAN";
            var date = new DateTime(2025, 12, 20);

            _mockFlightRepo.Setup(repo => repo.SearchFlightsAsync(from, to, date))
                           .ReturnsAsync(fakeFlights);

            // --- ACT ---
            var result = await _controller.SearchFlights(from, to, date);

            // --- ASSERT ---
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedFlights = Assert.IsAssignableFrom<IEnumerable<Flight>>(okResult.Value);
            Assert.Single(returnedFlights); // Khẳng định: Tìm thấy 1 chuyến bay
        }

        // --- Test 2: Tìm kiếm không thấy kết quả ---
        [Fact]
        public async Task SearchFlights_Returns_Ok_With_EmptyList()
        {
            // --- ARRANGE ---
            var fakeFlights = new List<Flight>(); // Danh sách rỗng
            var from = "SGN";
            var to = "HAN";
            var date = new DateTime(2025, 12, 21);

            _mockFlightRepo.Setup(repo => repo.SearchFlightsAsync(from, to, date))
                           .ReturnsAsync(fakeFlights);

            // --- ACT ---
            var result = await _controller.SearchFlights(from, to, date);

            // --- ASSERT ---
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedFlights = Assert.IsAssignableFrom<IEnumerable<Flight>>(okResult.Value);
            Assert.Empty(returnedFlights); // Khẳng định: Danh sách rỗng
        }

        // --- Test 3: Admin tạo chuyến bay (POST) ---
        // (Sửa lỗi 'Create' vs 'PostFlight' của bạn)
        [Fact]
        // Đổi tên Test Case (tùy chọn)
        public async Task PostFlight_Returns_CreatedAtActionResult()
        {
            // --- ARRANGE ---
            var newFlight = new Flight { FlightId = 100, FlightNumber = "NEW01" };

            // Replace this line in PostFlight_Returns_CreatedAtActionResult test:
            // var result = await _controller.Create(newFlight);
            // With the correct method name from FlightsController:
            var result = await _controller.Create(newFlight); 

            // --- ASSERT ---
            // 1. Khẳng định: Kết quả là 201 Created
            // Thêm .Result (vì PostFlight trả về ActionResult<Flight>)
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // 2. Khẳng định: Dữ liệu trả về là chuyến bay mới
            var returnedFlight = Assert.IsType<Flight>(createdResult.Value);
            Assert.Equal("NEW01", returnedFlight.FlightNumber);

            // 3. Khẳng định: Controller PHẢI GỌI hàm AddAsync của Repo 1 LẦN
            _mockFlightRepo.Verify(repo => repo.AddAsync(newFlight), Times.Once);
        }

        // --- Test 4: Admin lấy chuyến bay (GET by ID) không thấy ---
        [Fact]
        // Đổi tên Test Case (tùy chọn)
        public async Task GetFlight_Returns_NotFound_When_Flight_Missing()
        {
            // --- ARRANGE ---
            // Dạy Repo: Khi GetById(99), trả về NULL
            // (Hàm trong Repo vẫn là GetByIdAsync, chỉ Controller đổi tên)
            _mockFlightRepo.Setup(repo => repo.GetByIdAsync(99))
                           .ReturnsAsync((Flight)null);

            // Replace this line in GetFlight_Returns_NotFound_When_Flight_Missing test:
            // var result = await _controller.GetFlight(99);
            // With the correct method name from FlightsController:
            var result = await _controller.GetById(99);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result.Result); // Khẳng định: Là 404 Not Found
        }
    }
}