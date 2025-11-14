using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IAirportRepository
    {
        Task<Airport?> GetByIdAsync(string id);
        Task<IEnumerable<Airport>> GetAllAsync();
        Task AddAsync(Airport airport);
        Task UpdateAsync(Airport airport);
        Task DeleteAsync(string id);
    }
}
