using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Khóa
    public class AirlinesController : ControllerBase
    {
        private readonly IAirlineRepository _airlineRepo;
        public AirlinesController(IAirlineRepository airlineRepo) { _airlineRepo = airlineRepo; }

        // GET: /api/airlines
        [HttpGet]
        [AllowAnonymous] // 2. Mở (Public)
        public async Task<ActionResult<IEnumerable<Airline>>> GetAll()
            => Ok(await _airlineRepo.GetAllAsync());

        // --- PHẦN ADMIN ---
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Airline>> GetById(string id)
        {
            var airline = await _airlineRepo.GetByIdAsync(id);
            if (airline == null) return NotFound(new { Message = $"Không tìm thấy Airline với ID {id}" });
            return Ok(airline);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Airline airline)
        {
            await _airlineRepo.AddAsync(airline);
            return CreatedAtAction(nameof(GetById), new { id = airline.AirlineCode }, airline);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(string id, Airline airline)
        {
            if (id != airline.AirlineCode) return BadRequest();
            await _airlineRepo.UpdateAsync(airline);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            await _airlineRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
