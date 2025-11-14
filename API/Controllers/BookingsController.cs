using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;

        public BookingsController(IBookingRepository bookingRepository)
        {
            _bookingRepo = bookingRepository;
        }

        // POST: /api/bookings (Nghiệp vụ Đặt vé)
        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking(BookingRequestDto request)
        {
            if (request.Passengers == null || !request.Passengers.Any())
            {
                return BadRequest("Phải có ít nhất 1 hành khách");
            }

            try
            {
                // Hàm CreateBookingAsync trong Repository
                // sẽ xử lý toàn bộ logic nghiệp vụ phức tạp
                var newBooking = await _bookingRepo.CreateBookingAsync(
                    request.UserId,
                    request.FlightId,
                    request.SeatClassId,
                    request.Passengers);

                return Ok(newBooking);
            }
            catch (Exception ex)
            {
                // Bắt các lỗi nghiệp vụ (ví dụ: Hết vé)
                return BadRequest(ex.Message);
            }
        }

        // GET: /api/bookings/user/5 (Lịch sử đặt vé)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByUser(int userId)
        {
            var bookings = await _bookingRepo.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }

        // GET: /api/bookings
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll()
        {
            return Ok(await _bookingRepo.GetAllAsync());
        }

        // DELETE: /api/bookings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _bookingRepo.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// GET: /api/bookings/report/revenue?from=2025-01-01&to=2025-01-31
        /// </summary>
        //[HttpGet("report/revenue")]
        //public async Task<ActionResult<decimal>> GetRevenueReport(
        //    [FromQuery] DateTime from,
        //    [FromQuery] DateTime to)
        //{
        //    var revenue = await _bookingRepo.GetRevenueReportAsync(from, to);
        //    return Ok(new { TotalRevenue = revenue });
        //}
    }
}
