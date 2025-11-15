using BusinessObject;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        // --- Chuẩn CRUD ---
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<User?> UpdateUserProfileAsync(int userId, UserProfileUpdateDto userDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        // --- Đặc thù ---
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<int> GetRegisteredUsersCountAsync();
    }
}
