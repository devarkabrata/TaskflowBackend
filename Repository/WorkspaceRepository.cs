using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly AppDBContext _context;

        public WorkspaceRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Workspace?> GetByIdAsync(Guid id)
            => await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == id);

        public async Task<Workspace?> GetByOwnerIdAsync(Guid ownerId)
            => await _context.Workspaces.FirstOrDefaultAsync(w => w.OwnerId == ownerId);

        public async Task<int> GetCountByOwnerIdAsync(Guid ownerId)
            => await _context.Workspaces.CountAsync(w => w.OwnerId == ownerId);

        public async Task<Workspace> CreateAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }
    }
}
