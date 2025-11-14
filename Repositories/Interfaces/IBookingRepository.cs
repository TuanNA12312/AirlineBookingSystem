using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IBookingRepository
    {
        // --- Chuẩn CRUD ---
        Task<Booking?> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);
        // --- Đặc thù ---
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
        Task<Booking> CreateBookingAsync(int userId, int flightId, int seatClassId, List<Passenger> passengers);
        Task<decimal> GetRevenueReportAsync(DateTime from, DateTime to);
        Task<int> GetTotalBookingsCountAsync();
    }
}
