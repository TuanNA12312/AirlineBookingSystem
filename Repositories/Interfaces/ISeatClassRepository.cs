using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISeatClassRepository {
        Task<SeatClass?> GetByIdAsync(int id);
        Task<IEnumerable<SeatClass>> GetAllAsync();
        Task AddAsync(SeatClass seatClass);
        Task UpdateAsync(SeatClass seatClass);
        Task DeleteAsync(int id);
    }
}
