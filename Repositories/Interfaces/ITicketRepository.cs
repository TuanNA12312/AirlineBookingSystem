using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ITicketRepository { 
        Task<IEnumerable<Ticket>> GetAllAsync(); 
        Task<Ticket?> GetByIdAsync(int id); 
        Task AddAsync(Ticket ticket); 
        Task UpdateAsync(Ticket ticket); 
        Task DeleteAsync(int id); 
    }
}
