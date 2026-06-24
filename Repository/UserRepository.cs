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
        public async Task<User?> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        // Updating user
        public async Task<User?> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
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