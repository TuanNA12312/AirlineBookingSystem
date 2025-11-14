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
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPassengerRepository _passengerRepo;

        public BookingRepository(ApplicationDbContext context, IPassengerRepository passengerRepo)
        {
            _context = context;
            _passengerRepo = passengerRepo;
        }

        public async Task<Booking> CreateBookingAsync(int userId, int flightId, int seatClassId, List<Passenger> passengers)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var flightPrice = await _context.FlightPrices.FirstOrDefaultAsync(p => p.FlightId == flightId && p.SeatClassId == seatClassId);
                if (flightPrice == null) throw new Exception("Không tìm thấy giá vé.");

                decimal pricePerTicket = flightPrice.Price;
                decimal totalPrice = pricePerTicket * passengers.Count;

                var flight = await _context.Flights.FindAsync(flightId);
                if (flight == null) throw new Exception("Không tìm thấy chuyến bay.");
                if (flight.AvailableSeats < passengers.Count) throw new Exception("Không đủ vé.");

                flight.AvailableSeats -= passengers.Count;
                _context.Flights.Update(flight);

                List<int> passengerIds = new List<int>();
                foreach (var p in passengers)
                {
                    await _passengerRepo.AddAsync(p); // Dùng repo để thêm
                    passengerIds.Add(p.PassengerId);
                }

                var newBooking = new Booking
                {
                    UserId = userId,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = totalPrice,
                    BookingCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                    Status = "Confirmed"
                };
                await _context.Bookings.AddAsync(newBooking);
                await _context.SaveChangesAsync();

                foreach (var passengerId in passengerIds)
                {
                    var newTicket = new Ticket
                    {
                        BookingId = newBooking.BookingId,
                        FlightId = flightId,
                        PassengerId = passengerId,
                        SeatClassId = seatClassId,
                        PricePaid = pricePerTicket
                    };
                    await _context.Tickets.AddAsync(newTicket);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return newBooking;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi tạo booking: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Tickets).ThenInclude(t => t.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Tickets).ThenInclude(t => t.Passenger)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<decimal> GetRevenueReportAsync(DateTime from, DateTime to)
        {
            return await _context.Bookings
                .Where(b => b.Status == "Confirmed" && b.BookingDate >= from && b.BookingDate <= to)
                .SumAsync(b => b.TotalPrice);
        }

        public async Task<int> GetTotalBookingsCountAsync()
        {
            return await _context.Bookings.Where(b => b.Status == "Confirmed").CountAsync();
        }

        // --- Chuẩn CRUD ---
        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tickets).ThenInclude(t => t.Passenger)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }
        public async Task<IEnumerable<Booking>> GetAllAsync() => await _context.Bookings.Include(b => b.User).AsNoTracking().ToListAsync();
        public async Task AddAsync(Booking booking) { _context.Bookings.Add(booking); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Booking booking) { _context.Entry(booking).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var b = await GetByIdAsync(id); if (b != null) { _context.Bookings.Remove(b); await _context.SaveChangesAsync(); } }
    }
}
