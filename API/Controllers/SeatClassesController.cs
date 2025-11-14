using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SeatClassesController : ControllerBase
    {
        private readonly ISeatClassRepository _seatClassRepo;
        public SeatClassesController(ISeatClassRepository seatClassRepo)
        {
            _seatClassRepo = seatClassRepo;
        }

        // GET: /api/seatclasses
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatClass>>> GetAllPublic()
            => Ok(await _seatClassRepo.GetAllAsync());

        // GET: /api/seatclasses/1
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeatClass>> GetById(int id)
            => Ok(await _seatClassRepo.GetByIdAsync(id));

        // POST: /api/seatclasses
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(SeatClass seatClass)
        {
            await _seatClassRepo.AddAsync(seatClass);
            return CreatedAtAction(nameof(GetById), new { id = seatClass.SeatClassId }, seatClass);
        }

        // PUT: /api/seatclasses/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, SeatClass seatClass)
        {
            if (id != seatClass.SeatClassId) return BadRequest();
            await _seatClassRepo.UpdateAsync(seatClass);
            return NoContent();
        }

        // DELETE: /api/seatclasses/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _seatClassRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
