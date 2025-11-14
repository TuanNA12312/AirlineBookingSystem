using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Khóa
    public class AirportsController : ControllerBase
    {
        private readonly IAirportRepository _airportRepo;
        public AirportsController(IAirportRepository airportRepo) { _airportRepo = airportRepo; }

        // GET: /api/airports
        [HttpGet]
        [AllowAnonymous] // 2. Mở khóa (Public)
        public async Task<ActionResult<IEnumerable<Airport>>> GetAll()
            => Ok(await _airportRepo.GetAllAsync());

        // --- PHẦN ADMIN ---
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Airport>> GetById(string id)
        {
            var airport = await _airportRepo.GetByIdAsync(id);
            if (airport == null) return NotFound(new { Message = $"Không tìm thấy Airport với ID {id}" });
            return Ok(airport);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Airport airport)
        {
            await _airportRepo.AddAsync(airport);
            return CreatedAtAction(nameof(GetById), new { id = airport.AirportCode }, airport);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(string id, Airport airport)
        {
            if (id != airport.AirportCode) return BadRequest();
            await _airportRepo.UpdateAsync(airport);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            await _airportRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
