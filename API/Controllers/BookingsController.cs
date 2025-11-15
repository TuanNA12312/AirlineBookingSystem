using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
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

        [HttpGet("history/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetUserHistory(int userId)
        {
            // Kiểm tra phân quyền: User chỉ được xem Booking của chính họ
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != userId.ToString())
            {
                // Giả sử ClaimTypes.NameIdentifier lưu UserId (int)
                // Hoặc bạn cần điều chỉnh theo cách lưu Claim của mình
                return Forbid("Bạn không có quyền xem lịch sử của User này.");
            }

            var bookings = await _bookingRepo.GetUserBookingHistoryAsync(userId);
            return Ok(bookings);
        }

        // GET: /api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingDetails(int id)
        {
            var booking = await _bookingRepo.GetBookingDetailsAsync(id);
            if (booking == null) return NotFound("Không tìm thấy đơn đặt chỗ.");

            // Kiểm tra phân quyền: User chỉ được xem Booking của chính họ
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != booking.UserId && !User.IsInRole("Admin"))
            {
                return Forbid("Bạn không có quyền xem chi tiết đơn đặt chỗ này.");
            }

            return Ok(booking);
        }

        // POST: /api/bookings/{id}/cancel (Cho MVC gọi để hủy vé)
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _bookingRepo.GetBookingDetailsAsync(id);
            if (booking == null) return NotFound("Không tìm thấy đơn đặt chỗ.");

            // Kiểm tra phân quyền
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != booking.UserId && !User.IsInRole("Admin"))
            {
                return Forbid("Bạn không có quyền hủy đơn đặt chỗ này.");
            }

            // Kiểm tra trạng thái hiện tại (chỉ được hủy nếu là Confirmed hoặc Pending)
            if (booking.Status == "Cancelled" || booking.Status == "Completed")
            {
                return BadRequest("Đơn đặt chỗ này không thể hủy được nữa.");
            }

            // Cập nhật trạng thái qua Repository
            var success = await _bookingRepo.UpdateBookingStatusAsync(id, "Cancelled");

            if (success)
            {
                // Logic hoàn tiền (tạm thời bỏ qua theo yêu cầu)
                return Ok(new { Message = "Hủy đơn đặt chỗ thành công." });
            }

            return BadRequest("Hủy đơn đặt chỗ thất bại.");
        }
    }
}

