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
    public class AirportRepository : IAirportRepository
    {
        private readonly ApplicationDbContext _context;
        public AirportRepository(ApplicationDbContext context) { _context = context; }

        // --- Chuẩn CRUD (ID là string) ---
        public async Task<Airport?> GetByIdAsync(string id) => await _context.Airports.FindAsync(id);
        public async Task<IEnumerable<Airport>> GetAllAsync() => await _context.Airports.AsNoTracking().ToListAsync();
        public async Task AddAsync(Airport airport) { _context.Airports.Add(airport); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Airport airport) { _context.Entry(airport).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(string id) { var a = await GetByIdAsync(id); if (a != null) { _context.Airports.Remove(a); await _context.SaveChangesAsync(); } }
    }
}
