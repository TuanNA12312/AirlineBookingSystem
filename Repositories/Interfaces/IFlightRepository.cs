using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IFlightRepository
    {
        // --- Chuẩn CRUD ---
        Task<Flight?> GetByIdAsync(int id);
        Task<IEnumerable<Flight>> GetAllAsync();
        Task AddAsync(Flight flight);
        Task UpdateAsync(Flight flight);
        Task DeleteAsync(int id);
        // --- Đặc thù ---
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCode, string arrivalCode, DateTime departureDate);
        Task<int> GetTotalFlightsCountAsync();
    }
}
