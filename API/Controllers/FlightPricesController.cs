using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Khóa toàn bộ
    public class FlightPricesController : ControllerBase
    {
        private readonly IFlightPriceRepository _priceRepo;
        public FlightPricesController(IFlightPriceRepository priceRepo) { _priceRepo = priceRepo; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPrice>>> GetAll()
            => Ok(await _priceRepo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPrice>> GetById(int id)
        {
            var price = await _priceRepo.GetByIdAsync(id);
            if (price == null) return NotFound(new { Message = $"Không tìm thấy FlightPrice với ID {id}" });
            return Ok(price);
        }

        [HttpPost]
        public async Task<ActionResult> Create(FlightPrice price)
        {
            await _priceRepo.AddAsync(price);
            return CreatedAtAction(nameof(GetById), new { id = price.FlightPriceId }, price);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, FlightPrice price)
        {
            if (id != price.FlightPriceId) return BadRequest();
            await _priceRepo.UpdateAsync(price);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _priceRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
