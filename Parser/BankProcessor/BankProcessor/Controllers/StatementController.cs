using Microsoft.AspNetCore.Mvc;
using BankProcessor.API.Statement;
using BankProcessor.Models;

namespace BankProcessor.Controllers
{
    [ApiController]
    public class StatementController : ControllerBase
    {
        private readonly StatementContext? _dbContext;

        public StatementController(StatementContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("/statement")]
        public async Task<IActionResult> CreateStatement([FromForm] CreateStatementRequest request)
        {
            IFormFile? file = request.payload.Statement;

            // Check if file is selected
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    uuid = (string?)null,
                    message = "File is not a valid pdf"
                });
            }

            // Check if file is pdf
            //var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            //if (string.IsNullOrEmpty(extension) || (extension != ".pdf"))
            if (file.ContentType != "application/pdf")
            {
                return BadRequest(new
                {
                    uuid = (string?)null,
                    message = "File is not a valid pdf"
                });
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save metadata to the database
            var fileRecord = new FileRecord
            {
                FileName = file.FileName,
                FilePath = filePath
            };

            _dbContext.Files.Add(fileRecord);
            await _dbContext.SaveChangesAsync();

            return Ok(new { uuid = fileRecord.Id, message = "Success" });
        }
    }
}
