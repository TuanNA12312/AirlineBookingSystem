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
    public class FlightRepository : IFlightRepository
    {
        private readonly ApplicationDbContext _context;
        public FlightRepository(ApplicationDbContext context) { _context = context; }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCode, string arrivalCode, DateTime departureDate)
        {
            var searchDate = departureDate.Date;
            return await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Airline)
                .Include(f => f.Prices).ThenInclude(p => p.SeatClass)
                .Where(f =>
                    f.DepartureAirportCode == departureCode &&
                    f.ArrivalAirportCode == arrivalCode &&
                    f.DepartureTime.Date == searchDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalFlightsCountAsync()
        {
            return await _context.Flights.CountAsync();
        }

        public async Task<IEnumerable<Flight>> GetAllAsync() => await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .AsNoTracking()
            .ToListAsync();

        public async Task<Flight?> GetByIdAsync(int id) => await _context.Flights.FindAsync(id);
        public async Task AddAsync(Flight flight) { _context.Flights.Add(flight); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Flight flight) { _context.Entry(flight).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var f = await GetByIdAsync(id); if (f != null) { _context.Flights.Remove(f); await _context.SaveChangesAsync(); } }
    }
}
