using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Models;



namespace TaskFlowBackend.Data
{
    public class ArchiveDBContext : DbContext
    {
        // Initiating DB Context from EF Core
        public ArchiveDBContext(DbContextOptions<ArchiveDBContext> options) : base(options) { }
        public DbSet<ArchivedTaskItem> ArchivedTaskItems => Set<ArchivedTaskItem>();
    }
}
