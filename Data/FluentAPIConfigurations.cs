using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Data.Fluent
{
    public class FluentAPIConfigurations
    {
        public static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Title)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValue(string.Empty);

                entity.Property(u => u.AvatarInitials)
                    .IsRequired()
                    .HasMaxLength(2);

                entity.Property(u => u.AvatarUrl)
                    .HasMaxLength(500);

                entity.Property(u => u.AvatarPublicId)
                    .HasMaxLength(300);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(u => u.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");
            });
        }
    }
}