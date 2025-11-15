using BusinessObject;
using DataAccess;
using Microsoft.AspNetCore.Identity;
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

        public async Task<User?> UpdateUserProfileAsync(int userId, UserProfileUpdateDto userDto)
        {
            var userInDb = await GetByIdAsync(userId);
            if (userInDb == null)
            {
                return null;
            }

            userInDb.FullName = userDto.FullName;
            userInDb.Email = userDto.Email;
            userInDb.PhoneNumber = userDto.PhoneNumber;

            try
            {
                // Đánh dấu entity là đã sửa và lưu
                _context.Entry(userInDb).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return userInDb;
            }
            catch (DbUpdateException)
            {
                // Xử lý lỗi (ví dụ: email bị trùng, nếu bạn cấu hình unique index)
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var userInDb = await GetByIdAsync(changePasswordDto.UserId);
            if (userInDb == null)
            {
                return false; // Không tìm thấy user
            }

            // KIỂM TRA MẬT KHẨU CŨ (So sánh string thô)
            if (userInDb.PasswordHash != changePasswordDto.OldPassword)
            {
                return false; // Sai mật khẩu cũ
            }

            // CẬP NHẬT MẬT KHẨU MỚI
            userInDb.PasswordHash = changePasswordDto.NewPassword;

            // Lưu lại
            _context.Entry(userInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
