using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IAirlineRepository {
        Task<Airline?> GetByIdAsync(string id);
        Task<IEnumerable<Airline>> GetAllAsync();
        Task AddAsync(Airline airline);
        Task UpdateAsync(Airline airline);
        Task DeleteAsync(string id);
    }
}
