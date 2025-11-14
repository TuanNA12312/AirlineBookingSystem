using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightRepository _flightRepo;

        public FlightsController(IFlightRepository flightRepository)
        {
            _flightRepo = flightRepository;
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Flight>>> SearchFlights(
            [FromQuery] string departureCode,
            [FromQuery] string arrivalCode,
            [FromQuery] DateTime departureDate)
        {
            try
            {
                var flights = await _flightRepo.SearchFlightsAsync(departureCode, arrivalCode, departureDate);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server nội bộ: {ex.Message}");
            }
        }

        // GET: /api/flights
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetAllFlights()
        {
            return Ok(await _flightRepo.GetAllAsync());
        }

        // GET: /api/flights/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Flight>> GetFlightById(int id)
        {
            var flight = await _flightRepo.GetByIdAsync(id);
            if (flight == null)
            {
                return NotFound("Không tìm thấy chuyến bay");
            }
            return Ok(flight);
        }

        // POST: /api/flights
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Flight>> CreateFlight(Flight flight)
        {
            try
            {
                await _flightRepo.AddAsync(flight);
                // Trả về 201 Created và link để lấy resource
                return CreatedAtAction(nameof(GetFlightById), new { id = flight.FlightId }, flight);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: /api/flights/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFlight(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                return BadRequest("ID không khớp");
            }
            try
            {
                await _flightRepo.UpdateAsync(flight);
                return NoContent(); // 204 No Content - Thành công
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound("Không tìm thấy chuyến bay để cập nhật");
            }
        }

        // DELETE: /api/flights/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            try
            {
                await _flightRepo.DeleteAsync(id);
                return NoContent(); // 204 No Content - Thành công
            }
            catch (Exception)
            {
                return NotFound("Không tìm thấy chuyến bay để xoá");
            }
        }
    }
}
