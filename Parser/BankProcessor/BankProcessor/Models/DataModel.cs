using System.ComponentModel.DataAnnotations;

namespace BankProcessor.Models
{
    public class Account
    {
        public Guid Uuid { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public DateOnly StatementDate { get; set; }

        public int TransactionCount { get; set; } = 0; // Default is 0

        // Navigation property for the relationship
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; } // Auto-increment primary key

        [Required]
        public Guid AccountUuid { get; set; } // Foreign key link to Statement

        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public decimal Balance { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        // Navigation property for the relationship
        public Account? Account { get; set; }
    }
}