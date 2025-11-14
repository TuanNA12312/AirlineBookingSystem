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
    public class SeatClassesControllerTests
    {
        private readonly Mock<ISeatClassRepository> _mockSeatClassRepo;
        private readonly SeatClassesController _controller;
        public SeatClassesControllerTests()
        {
            _mockSeatClassRepo = new Mock<ISeatClassRepository>();
            _controller = new SeatClassesController(_mockSeatClassRepo.Object);
        }

        // --- Test 1: Lấy tất cả (Public) ---
        [Fact]
        public async Task GetAllPublic_Returns_Ok_With_SeatClasses()
        {
            // ARRANGE
            _mockSeatClassRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<SeatClass> { new SeatClass() });
            // ACT
            var result = await _controller.GetAll();
            // ASSERT
            Assert.IsType<OkObjectResult>(result.Result);
        }

        // --- Test 2: Admin GetById (Không tìm thấy) ---
        [Fact]
        public async Task GetById_Returns_NotFound_When_SeatClass_Missing()
        {
            // ARRANGE
            _mockSeatClassRepo.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((SeatClass)null);
            // ACT
            var result = await _controller.GetById(99);
            // ASSERT
            // SỬA LỖI: Mong đợi NotFoundObjectResult
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
