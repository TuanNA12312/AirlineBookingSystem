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
    public class PassengerRepository : IPassengerRepository
    {
        private readonly ApplicationDbContext _context;
        public PassengerRepository(ApplicationDbContext context) 
        { 
            _context = context; 
        }

        public async Task<IEnumerable<Passenger>> GetAllAsync() => await _context.Passengers.AsNoTracking().ToListAsync();
        public async Task<Passenger?> GetByIdAsync(int id) => await _context.Passengers.FindAsync(id);
        public async Task AddAsync(Passenger passenger) 
        { 
            _context.Passengers.Add(passenger); 
            await _context.SaveChangesAsync(); 
        }

        public async Task UpdateAsync(Passenger passenger) 
        { 
            _context.Entry(passenger).State = EntityState.Modified; 
            await _context.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(int id) 
        { 
            var p = await GetByIdAsync(id); 
            if (p != null) 
            { 
                _context.Passengers.Remove(p); 
                await _context.SaveChangesAsync(); 
            } 
        }
    }
}
