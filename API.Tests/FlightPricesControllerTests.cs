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
    public class FlightPricesControllerTests
    {
        private readonly Mock<IFlightPriceRepository> _mockPriceRepo;
        private readonly FlightPricesController _controller;
        public FlightPricesControllerTests()
        {
            _mockPriceRepo = new Mock<IFlightPriceRepository>();
            _controller = new FlightPricesController(_mockPriceRepo.Object);
        }

        // --- Test 1: Admin GetById (Không tìm thấy) ---
        [Fact]
        public async Task GetById_Returns_NotFound_When_Price_Missing()
        {
            // ARRANGE
            _mockPriceRepo.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((FlightPrice)null);
            // ACT
            var result = await _controller.GetById(99);
            // ASSERT
            // SỬA LỖI: Mong đợi NotFoundObjectResult
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
