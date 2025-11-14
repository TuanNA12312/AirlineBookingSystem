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
    public class AirlineRepository : IAirlineRepository
    {
        private readonly ApplicationDbContext _context;
        public AirlineRepository(ApplicationDbContext context) { _context = context; }

        // --- Chuẩn CRUD (ID là string) ---
        public async Task<Airline?> GetByIdAsync(string id) => await _context.Airlines.FindAsync(id);
        public async Task<IEnumerable<Airline>> GetAllAsync() => await _context.Airlines.AsNoTracking().ToListAsync();
        public async Task AddAsync(Airline airline) { _context.Airlines.Add(airline); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Airline airline) { _context.Entry(airline).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(string id) { var a = await GetByIdAsync(id); if (a != null) { _context.Airlines.Remove(a); await _context.SaveChangesAsync(); } }
    }
}
