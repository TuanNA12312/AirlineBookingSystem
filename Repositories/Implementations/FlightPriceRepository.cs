using BusinessObject;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class FlightPriceRepository : IFlightPriceRepository
    {
        private readonly ApplicationDbContext _context;
        public FlightPriceRepository(ApplicationDbContext context) { _context = context; }

        public async Task<IEnumerable<FlightPrice>> GetByFlightIdAsync(int flightId)
        {
            return await _context.FlightPrices
                .Where(p => p.FlightId == flightId)
                .Include(p => p.SeatClass)
                .AsNoTracking()
                .ToListAsync();
        }

        // --- Chuẩn CRUD ---
        public async Task<FlightPrice?> GetByIdAsync(int id) => await _context.FlightPrices.FindAsync(id);
        public async Task<IEnumerable<FlightPrice>> GetAllAsync() => await _context.FlightPrices.AsNoTracking().ToListAsync();
        public async Task AddAsync(FlightPrice flightPrice) { _context.FlightPrices.Add(flightPrice); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(FlightPrice flightPrice) { _context.Entry(flightPrice).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var fp = await GetByIdAsync(id); if (fp != null) { _context.FlightPrices.Remove(fp); await _context.SaveChangesAsync(); } }
    }
}
