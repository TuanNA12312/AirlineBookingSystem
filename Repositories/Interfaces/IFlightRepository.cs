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
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departureCode, string arrivalCode, DateTime departureDate); // (Tìm kiếm)
        Task<IEnumerable<Flight>> GetAllAsync();
        Task<Flight?> GetByIdAsync(int id);
        Task AddAsync(Flight flight);
        Task UpdateAsync(Flight flight);
        Task DeleteAsync(int id);
        Task<int> GetTotalFlightsCountAsync();
    }
}
