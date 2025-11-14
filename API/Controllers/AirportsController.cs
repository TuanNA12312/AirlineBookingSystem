using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AirportsController : ControllerBase
    {
        private readonly IAirportRepository _airportRepo;

        public AirportsController(IAirportRepository airportRepository)
        {
            _airportRepo = airportRepository;
        }

        // GET: /api/airports
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Airport>>> GetAllAirports()
        {
            return Ok(await _airportRepo.GetAllAsync());
        }

        // GET: /api/airports/SGN
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Airport>> GetById(string id)
            => Ok(await _airportRepo.GetByIdAsync(id));

        // POST: /api/airports
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Airport airport)
        {
            await _airportRepo.AddAsync(airport);
            return CreatedAtAction(nameof(GetById), new { id = airport.AirportCode }, airport);
        }

        // PUT: /api/airports/SGN
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(string id, Airport airport)
        {
            if (id != airport.AirportCode) return BadRequest();
            await _airportRepo.UpdateAsync(airport);
            return NoContent();
        }

        // DELETE: /api/airports/SGN
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            await _airportRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
