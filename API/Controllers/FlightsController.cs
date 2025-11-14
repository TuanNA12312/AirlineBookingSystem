using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Khóa toàn bộ
    public class FlightsController : ControllerBase
    {
        private readonly IFlightRepository _flightRepo;
        public FlightsController(IFlightRepository flightRepo) { _flightRepo = flightRepo; }

        // GET: /api/flights/search
        [HttpGet("search")]
        [AllowAnonymous] // 2. Mở khóa cho Tìm kiếm (Public)
        public async Task<ActionResult<IEnumerable<Flight>>> SearchFlights(
            [FromQuery] string from, [FromQuery] string to, [FromQuery] DateTime date)
        {
            var flights = await _flightRepo.SearchFlightsAsync(from, to, date);
            return Ok(flights);
        }

        // GET: /api/flights/{id}
        // (User CẦN xem chi tiết để đặt vé)
        [HttpGet("{id}")]
        [AllowAnonymous] // 2. Mở khóa cho Public (Sửa lỗi logic)
        public async Task<ActionResult<Flight>> GetById(int id)
        {
            var flight = await _flightRepo.GetByIdAsync(id);
            if (flight == null) return NotFound(new { Message = $"Không tìm thấy Flight với ID {id}" });
            return Ok(flight);
        }

        // --- PHẦN ADMIN ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetAll()
            => Ok(await _flightRepo.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Flight flight)
        {
            await _flightRepo.AddAsync(flight);
            return CreatedAtAction(nameof(GetById), new { id = flight.FlightId }, flight);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, Flight flight)
        {
            if (id != flight.FlightId) return BadRequest("ID không khớp");
            await _flightRepo.UpdateAsync(flight);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _flightRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
