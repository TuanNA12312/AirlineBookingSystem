using API.Controllers;
using BusinessObject;
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
    public class AirportsControllerTests
    {
        private readonly Mock<IAirportRepository> _mockAirportRepo;
        private readonly AirportsController _controller;
        public AirportsControllerTests()
        {
            _mockAirportRepo = new Mock<IAirportRepository>();
            _controller = new AirportsController(_mockAirportRepo.Object);
        }

        // --- Test 1: Lấy tất cả (Public) ---
        [Fact]
        public async Task GetAllPublic_Returns_Ok_With_Airports()
        {
            // ARRANGE
            _mockAirportRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Airport> { new Airport() });
            // ACT
            // FIX: Call the actual controller method name GetAllAirports()
            var result = await _controller.GetAll();
            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<IEnumerable<Airport>>(okResult.Value);
        }

        // --- Test 2: Admin GetById (Không tìm thấy) ---
        [Fact]
        public async Task GetById_Returns_NotFound_When_Airport_Missing()
        {
            // ARRANGE
            _mockAirportRepo.Setup(repo => repo.GetByIdAsync("XXX")).ReturnsAsync((Airport)null);
            // ACT
            var result = await _controller.GetById("XXX");
            // ASSERT
            // SỬA LỖI: Mong đợi NotFoundObjectResult
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
