using BusinessObject;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context) { _context = context; }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            // (Như đã thống nhất: so sánh chuỗi thẳng, không băm)
            return await Task.FromResult(user.PasswordHash == password);
        }

        public async Task<int> GetRegisteredUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        // --- Chuẩn CRUD ---
        public async Task<User?> GetByIdAsync(int id) => await _context.Users.FindAsync(id);
        public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.AsNoTracking().ToListAsync();
        public async Task AddAsync(User user) { _context.Users.Add(user); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(User user) { _context.Entry(user).State = EntityState.Modified; await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var u = await GetByIdAsync(id); if (u != null) { _context.Users.Remove(u); await _context.SaveChangesAsync(); } }
    }
}
