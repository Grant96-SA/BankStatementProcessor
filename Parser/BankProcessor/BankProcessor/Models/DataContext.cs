using Microsoft.EntityFrameworkCore;
using BankProcessor.Controllers;

namespace BankProcessor.Models
{
    public class StatementContext : DbContext
    {
        public DbSet<FileRecord>? Files { get; set; }
        public StatementContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FileRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FilePath).IsRequired();
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
