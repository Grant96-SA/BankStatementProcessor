using System.ComponentModel.DataAnnotations;

namespace BankProcessor.Models
{
    public class FileRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Auto-generate UUID
        public string FileName { get; set; } = string.Empty; // Original file name
        public string FilePath { get; set; } = string.Empty; // Path to local file
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Timestamp
    }
}