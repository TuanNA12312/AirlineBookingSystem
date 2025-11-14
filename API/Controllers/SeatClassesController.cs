using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Khóa
    public class SeatClassesController : ControllerBase
    {
        private readonly ISeatClassRepository _seatClassRepo;
        public SeatClassesController(ISeatClassRepository seatClassRepo) { _seatClassRepo = seatClassRepo; }

        // GET: /api/seatclasses
        [HttpGet]
        [AllowAnonymous] // 2. Mở (Public)
        public async Task<ActionResult<IEnumerable<SeatClass>>> GetAll()
            => Ok(await _seatClassRepo.GetAllAsync());

        // --- PHẦN ADMIN ---
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeatClass>> GetById(int id)
        {
            var seatClass = await _seatClassRepo.GetByIdAsync(id);
            if (seatClass == null) return NotFound(new { Message = $"Không tìm thấy SeatClass với ID {id}" });
            return Ok(seatClass);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(SeatClass seatClass)
        {
            await _seatClassRepo.AddAsync(seatClass);
            return CreatedAtAction(nameof(GetById), new { id = seatClass.SeatClassId }, seatClass);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, SeatClass seatClass)
        {
            if (id != seatClass.SeatClassId) return BadRequest();
            await _seatClassRepo.UpdateAsync(seatClass);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _seatClassRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
