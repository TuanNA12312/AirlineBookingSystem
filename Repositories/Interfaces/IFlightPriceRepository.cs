using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IFlightPriceRepository {
        // --- Chuẩn CRUD ---
        Task<FlightPrice?> GetByIdAsync(int id);
        Task<IEnumerable<FlightPrice>> GetAllAsync();
        Task AddAsync(FlightPrice flightPrice);
        Task UpdateAsync(FlightPrice flightPrice);
        Task DeleteAsync(int id);
        // --- Đặc thù ---
        Task<IEnumerable<FlightPrice>> GetByFlightIdAsync(int flightId);
    }
}
