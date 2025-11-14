using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IFlightPriceRepository { 
        Task<IEnumerable<FlightPrice>> GetAllAsync(); 
        Task<FlightPrice?> GetByIdAsync(int id); 
        Task AddAsync(FlightPrice flightPrice); 
        Task UpdateAsync(FlightPrice flightPrice); 
        Task DeleteAsync(int id); 
    }
}
