using API.Controllers;
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
    public class ReportsControllerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IFlightRepository> _mockFlightRepo;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockFlightRepo = new Mock<IFlightRepository>();
            _controller = new ReportsController(_mockBookingRepo.Object, _mockUserRepo.Object, _mockFlightRepo.Object);
        }

        // Replace all usages of 'result.Result' with just 'result' in the test methods
        [Fact]
        public async Task GetRevenueReport_Returns_Ok_With_Revenue()
        {
            // ARRANGE
            var request = new ReportRequestDto { FromDate = DateTime.Now, ToDate = DateTime.Now };
            _mockBookingRepo.Setup(repo => repo.GetRevenueReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(5000000); // Giả lập doanh thu 5 triệu
            // ACT
            var result = await _controller.GetRevenueReport(request);
            // ASSERT
            Assert.IsType<OkObjectResult>(result);
            // (Bạn có thể test giá trị trả về nếu muốn)
        }

        // --- Test 2: Báo cáo Dashboard ---
        [Fact]
        public async Task GetDashboardStats_Returns_Ok_With_Stats()
        {
            // ARRANGE
            _mockUserRepo.Setup(repo => repo.GetRegisteredUsersCountAsync()).ReturnsAsync(10);
            _mockBookingRepo.Setup(repo => repo.GetTotalBookingsCountAsync()).ReturnsAsync(20);
            _mockFlightRepo.Setup(repo => repo.GetTotalFlightsCountAsync()).ReturnsAsync(30);
            // ACT
            var result = await _controller.GetDashboardStats();
            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            // (Bạn có thể test giá trị trả về nếu muốn)
        }
    }
}
