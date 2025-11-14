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
    public class SeatClassRepository : ISeatClassRepository
    {
        private readonly ApplicationDbContext _context;
        public SeatClassRepository(ApplicationDbContext context) { _context = context; }

        // --- Chuẩn CRUD ---
        public async Task<SeatClass?> GetByIdAsync(int id) => await _context.SeatClasses.FindAsync(id);
        public async Task<IEnumerable<SeatClass>> GetAllAsync() => await _context.SeatClasses.AsNoTracking().ToListAsync();
        public async Task AddAsync(SeatClass seatClass) { _context.SeatClasses.Add(seatClass); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(SeatClass seatClass) { _context.Entry(seatClass).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var s = await GetByIdAsync(id); if (s != null) { _context.SeatClasses.Remove(s); await _context.SaveChangesAsync(); } }
    }
}
