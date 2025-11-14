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
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId); // (Lịch sử)
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);
        Task<Booking> CreateBookingAsync(int userId, int flightId, int seatClassId, List<Passenger> passengers);

        Task<decimal> GetRevenueReportAsync(DateTime from, DateTime to);

        Task<int> GetTotalBookingsCountAsync();
    }
}
