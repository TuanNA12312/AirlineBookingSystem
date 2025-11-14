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
    public class AirlinesControllerTests
    {
        private readonly Mock<IAirlineRepository> _mockAirlineRepo;
        private readonly AirlinesController _controller;
        public AirlinesControllerTests()
        {
            _mockAirlineRepo = new Mock<IAirlineRepository>();
            _controller = new AirlinesController(_mockAirlineRepo.Object);
        }

        // --- Test 1: Lấy tất cả (Public) ---
        [Fact]
        public async Task GetAllPublic_Returns_Ok_With_Airlines()
        {
            // ARRANGE
            _mockAirlineRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Airline> { new Airline() });
            // ACT
            var result = await _controller.GetAllPublic();
            // ASSERT
            Assert.IsType<OkObjectResult>(result.Result);
        }

        // --- Test 2: Admin GetById (Không tìm thấy) ---
        [Fact]
        public async Task GetById_Returns_NotFound_When_Airline_Missing()
        {
            // ARRANGE
            _mockAirlineRepo.Setup(repo => repo.GetByIdAsync("XX")).ReturnsAsync((Airline)null);
            // ACT
            var result = await _controller.GetById("XX");
            // ASSERT
            // SỬA LỖI: Mong đợi NotFoundObjectResult
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
