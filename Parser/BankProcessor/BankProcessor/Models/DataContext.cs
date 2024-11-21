using Microsoft.EntityFrameworkCore;
using BankProcessor.Models;

namespace BankProcessor.Models
{
    public class StatementContext : DbContext
    {
        public DbSet<Account> _Accounts { get; set; }
        public DbSet<Transaction> _Transactions {get; set;}

        public StatementContext(DbContextOptions<StatementContext> options) : base(options) 
        {
            // _Accounts = Accounts ?? throw new ArgumentNullException(nameof(Accounts));
            // _Transactions = Transactions ?? throw new ArgumentNullException(nameof(Transactions));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Uuid);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(50);
                entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.OpeningBalance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.ClosingBalance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.FromDate).HasColumnType("date");
                entity.Property(a => a.ToDate).HasColumnType("date");
                entity.Property(a => a.StatementDate).HasColumnType("date");
                entity.Property(a => a.TransactionCount).HasDefaultValue(0);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId); // Primary key 
                entity.Property(t => t.AccountUuid).IsRequired();
                entity.Property(t => t.Date).HasColumnType("date");
                entity.Property(t => t.Description).HasMaxLength(150);
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Balance).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Type).IsRequired().HasMaxLength(20);

                // Account linkage
                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountUuid)
                      .HasPrincipalKey(a => a.Uuid)
                      .OnDelete(DeleteBehavior.Cascade); 
            });
        }
    }
}
