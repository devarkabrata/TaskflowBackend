using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDBContext _context;

        public UserRepository(AppDBContext dbcontext)
        {
            _context = dbcontext;
        }
        private static string ComputeInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }

        // Getting user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return result;
        }

        // Getting user by id
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return result;
        }

        // Creating user
        public async Task<User?> CreateUserAsync(CreateUserRequestDto user)
        {
            // Making the password hash using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = user.Name,
                Title = user.Title ?? string.Empty,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                AvatarPublicId = user.AvatarPublicId ?? string.Empty,
                AvatarInitials = user.AvatarInitials ?? ComputeInitials(user.Name),
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
        
        // Updating user
        public async Task<User?> UpdateUserAsync(Guid id, UpdateUserRequestDto user)
        {
            var existingUser = await GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.Name = user.Name ?? existingUser.Name;
            existingUser.Title = user.Title ?? existingUser.Title;
            existingUser.AvatarUrl = user.AvatarUrl ?? existingUser.AvatarUrl;
            existingUser.AvatarPublicId = user.AvatarPublicId ?? existingUser.AvatarPublicId;
            existingUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        // Deleting user
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var existingUser = await GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return false;
            }

            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
            return true;
        }

        // Searching user by name or email or any future params
        public async Task<User?> GetUserSearchAsync(string searchTerm)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));
            return result;
        }
    }
}