using Microsoft.AspNetCore.Mvc;
using BankProcessor.API.Statement;
using BankProcessor.Models;
using BankProcessor.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace BankProcessor.Controllers
{
    [ApiController]
    public class StatementController : ControllerBase
    {
        private readonly StatementContext _dbContext;
        private readonly StatementProcessingService _processingService;

        public StatementController(StatementContext dbContext, StatementProcessingService processingService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
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
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || (extension != ".pdf"))
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

            string newFileName = Guid.NewGuid() + ".pdf";
            string filePath = Path.Combine(folderPath, newFileName);

            try
            {

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Queue the file path for processing by background service
                _processingService.QueueFile(filePath);
            }

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    uuid = (string?)null,
                    message = "Error processing statement: " + ex.Message
                });
            }

            // Return the result
            return Ok(new { uuid = Guid.NewGuid(), message = "Success" });
        }
    }
}
