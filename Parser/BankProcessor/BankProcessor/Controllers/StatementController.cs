using Microsoft.AspNetCore.Mvc;
using BankProcessor.API.Statement;
using BankProcessor.Models;
using BankProcessor.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankProcessor.API.Account;
using iText.StyledXmlParser.Jsoup.Parser;
using BankProcessor.API.Transaction;

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

        [HttpGet("/account/{uuid:guid}")]
        public async Task<IActionResult> GetAccount(Guid uuid)
        {
            // Retrieve the account from the database by UUID
            var account = await _dbContext._Accounts
                .FirstOrDefaultAsync(a => a.Uuid == uuid);

            // No statement
            if (account == null)
            {
                return NotFound(new GetAccountResponse(
                    Uuid: uuid,
                    Message: "Statement not found",
                    Data: null
                ));
            }

            // Account response
            var accountResponse = new GetAccountResponse
            (
                Uuid: uuid,
                Message: "Success",
                Data: new AccountData
                (
                    Name: account.Name,
                    Address: account.Address,
                    Account: new AccountDetails
                    (
                        Uuid: account.Uuid,
                        Number: account.AccountNumber,
                        Balance: account.Balance,
                        OpeningBalance: account.OpeningBalance,
                        ClosingBalance: account.ClosingBalance,
                        FromDate: account.FromDate,
                        ToDate: account.ToDate,
                        TransactionCount: account.TransactionCount,
                        StatementDate: account.StatementDate
                    )
                )
            );

            // Return the account details
            return Ok(accountResponse);
        }

        [HttpGet("/transactions/{uuid:guid}")]
        public async Task<IActionResult> GetTransactions(Guid uuid)
        {
            // Retrieve the account from the database by UUID
            var account = await _dbContext._Accounts
                .Include(a => a.Transactions)  // Include related transactions
                .FirstOrDefaultAsync(a => a.Uuid == uuid);

            // Check if account exists
            // No statement
            if (account == null)
            {
                return NotFound(new GetAccountResponse(
                    Uuid: uuid,
                    Message: "Statement not found",
                    Data: null
                ));
            }

            // Map transactions to TransactionData response model
            var transactionsData = account.Transactions.Select(t => new TransactionData
            (
                TransactionId : t.TransactionId,        
                Date : t.Date,                   
                Description : t.Description,     
                Amount : t.Amount,               
                Balance : t.Balance,             
                Type : t.Type                    
            )).ToList();

            // Return the response
            return Ok(new GetTransactionsResponse(
                Uuid: uuid,
                Message: "Success",
                Data: transactionsData
            ));
        }
    }
}
