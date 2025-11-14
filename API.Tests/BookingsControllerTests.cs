using API.Controllers;
using BusinessObject;
using Castle.Components.DictionaryAdapter.Xml;
using DataAccess;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Tests
{
    public class BookingsControllerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepo;
        private readonly BookingsController _controller;

        public BookingsControllerTests()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _controller = new BookingsController(_mockBookingRepo.Object);
        }

        // --- Test 1: Đặt vé thành công ---
        [Fact]
        public async Task CreateBooking_Returns_Ok_On_Success()
        {
            // --- ARRANGE ---
            var request = new BookingRequestDto
            {
                UserId = 1,
                FlightId = 1,
                SeatClassId = 1,
                Passengers = new List<Passenger> { new Passenger { FullName = "Test" } }
            };
            var fakeBooking = new Booking { BookingId = 1, TotalPrice = 1000 };

            // Dạy Repo: Khi CreateBookingAsync được gọi, trả về fakeBooking
            _mockBookingRepo.Setup(repo => repo.CreateBookingAsync(
                request.UserId,
                request.FlightId,
                request.SeatClassId,
                request.Passengers))
                .ReturnsAsync(fakeBooking);

            // --- ACT ---
            var result = await _controller.CreateBooking(request);

            // --- ASSERT ---
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooking = Assert.IsType<Booking>(okResult.Value);
            Assert.Equal(1, returnedBooking.BookingId); // Khẳng định: Đúng booking
        }

        // --- Test 2: Đặt vé thất bại (Hết vé / Lỗi nghiệp vụ) ---
        [Fact]
        public async Task CreateBooking_Returns_BadRequest_On_Repo_Exception()
        {
            // --- ARRANGE ---
            var request = new BookingRequestDto
            {
                UserId = 1,
                FlightId = 1,
                SeatClassId = 1,
                Passengers = new List<Passenger> { new Passenger { FullName = "Test" } }
            };

            // Dạy Repo: Khi CreateBookingAsync được gọi, HÃY NÉM RA LỖI "Hết vé"
            _mockBookingRepo.Setup(repo => repo.CreateBookingAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Passenger>>()))
                .ThrowsAsync(new Exception("Hết vé")); // <-- Ném lỗi

            // --- ACT ---
            var result = await _controller.CreateBooking(request);

            // --- ASSERT ---
            // 1. Khẳng định: Kết quả là 400 Bad Request
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);

            // 2. Khẳng định: Thông báo lỗi phải là "Hết vé"
            Assert.Equal("Hết vé", badRequestResult.Value);
        }

        // --- Test 3: Đặt vé thất bại (Không có hành khách) ---
        [Fact]
        public async Task CreateBooking_Returns_BadRequest_When_No_Passengers()
        {
            // --- ARRANGE ---
            var request = new BookingRequestDto
            {
                UserId = 1,
                FlightId = 1,
                SeatClassId = 1,
                Passengers = new List<Passenger>() // <-- Danh sách rỗng
            };

            // --- ACT ---
            var result = await _controller.CreateBooking(request);

            // --- ASSERT ---
            // 1. Khẳng định: Là 400 Bad Request
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);

            // 2. Khẳng định: Hàm CreateBookingAsync của Repo KHÔNG ĐƯỢC GỌI
            _mockBookingRepo.Verify(repo => repo.CreateBookingAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Passenger>>()), 
                Times.Never);
        }
    }
}
