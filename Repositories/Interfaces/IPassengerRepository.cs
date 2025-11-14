using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IPassengerRepository { 
        Task<IEnumerable<Passenger>> GetAllAsync(); 
        Task<Passenger?> GetByIdAsync(int id); 
        Task AddAsync(Passenger passenger); 
        Task UpdateAsync(Passenger passenger); 
        Task DeleteAsync(int id); 
    }
}
