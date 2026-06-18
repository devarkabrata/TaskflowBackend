using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data.Fluent;
using TaskFlowBackend.Models;



namespace TaskFlowBackend.Data
{
    public class AppDBContext : DbContext
    {
        // Initiating DB Context from EF Core
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations
            FluentAPIConfigurations.ConfigureUser(modelBuilder);
        }
    }
}
