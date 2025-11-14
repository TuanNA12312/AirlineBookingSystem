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
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketRepository(ApplicationDbContext context) 
        { 
            _context = context; 
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync() => await _context.Tickets.AsNoTracking().ToListAsync();
        public async Task<Ticket?> GetByIdAsync(int id) => await _context.Tickets.FindAsync(id);
        public async Task AddAsync(Ticket ticket) 
        { 
            _context.Tickets.Add(ticket); 
            await _context.SaveChangesAsync(); 
        }

        public async Task UpdateAsync(Ticket ticket) 
        { 
            _context.Entry(ticket).State = EntityState.Modified; 
            await _context.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(int id) 
        { 
            var t = await GetByIdAsync(id); 
            if (t != null) 
            { 
                _context.Tickets.Remove(t); 
                await _context.SaveChangesAsync(); 
            } 
        }
    }
}
