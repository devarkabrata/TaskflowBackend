using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDBContext _context;

        public RoleRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Roles>> GetAllEnabledAsync()
            => await _context.Roles
                .Where(r => r.IsEnable)
                .OrderBy(r => r.Name)
                .ToListAsync();

        public async Task<Roles?> GetByIdAsync(Guid id)
            => await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<bool> ExistsAndEnabledAsync(Guid id)
            => await _context.Roles.AnyAsync(r => r.Id == id && r.IsEnable);
    }
}
