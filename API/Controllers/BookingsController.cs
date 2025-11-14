using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Khóa (Phải đăng nhập mới được đặt/xem)
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;
        public BookingsController(IBookingRepository bookingRepository) { _bookingRepo = bookingRepository; }

        // POST: /api/bookings (User đặt vé)
        [HttpPost]
        public async Task<ActionResult<Booking>> Create(BookingRequestDto request)
        {
            if (request.Passengers == null || !request.Passengers.Any())
                return BadRequest("Phải có ít nhất 1 hành khách");
            try
            {
                var newBooking = await _bookingRepo.CreateBookingAsync(
                    request.UserId, request.FlightId, request.SeatClassId, request.Passengers);
                return Ok(newBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /api/bookings/user/5 (User xem lịch sử)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetByUserId(int userId)
        {
            // (Nâng cao: nên check userId từ Token)
            var bookings = await _bookingRepo.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }

        // --- PHẦN ADMIN ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll()
        {
            return Ok(await _bookingRepo.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Booking>> GetById(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return NotFound(new { Message = $"Không tìm thấy Booking với ID {id}" });
            return Ok(booking);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _bookingRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
