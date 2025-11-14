using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AirlinesController : ControllerBase
    {
        private readonly IAirlineRepository _airlineRepo;
        public AirlinesController(IAirlineRepository airlineRepo)
        {
            _airlineRepo = airlineRepo;
        }

        // GET: /api/airlines
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Airline>>> GetAllPublic()
            => Ok(await _airlineRepo.GetAllAsync());

        // GET: /api/airlines/VN
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Airline>> GetById(string id)
            => Ok(await _airlineRepo.GetByIdAsync(id));

        // POST: /api/airlines
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Airline airline)
        {
            await _airlineRepo.AddAsync(airline);
            return CreatedAtAction(nameof(GetById), new { id = airline.AirlineCode }, airline);
        }

        // PUT: /api/airlines/VN
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(string id, Airline airline)
        {
            if (id != airline.AirlineCode) return BadRequest();
            await _airlineRepo.UpdateAsync(airline);
            return NoContent();
        }

        // DELETE: /api/airlines/VN
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            await _airlineRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
