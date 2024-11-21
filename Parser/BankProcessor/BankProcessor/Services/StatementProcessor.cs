using BankProcessor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BankProcessor.Services
{
    public class StatementProcessingService : BackgroundService
    {
        // For storing file path
        private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();
        // Queue signal
        private readonly SemaphoreSlim _signal = new(0);
        private readonly IServiceScopeFactory _serviceProvider;

        // Since Singleton, I need to inject IServiceProvider and provide scope for StatementParser

        public StatementProcessingService(IServiceScopeFactory serviceProvider)
        {
            //_statementParser = statementParser;
            _serviceProvider = serviceProvider;
        }

        // Add file to the queue
        public void QueueFile(string filePath)
        {
            // Queue file path
            _fileQueue.Enqueue(filePath);
            Console.WriteLine($"File {filePath} added to the queue.");
            // New task available
            _signal.Release();
            Console.WriteLine("Semaphore released.");
        }

        // Background service logic
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _signal.WaitAsync(stoppingToken);

                // Get task from queue
                if (_fileQueue.TryDequeue(out var filePath))
                {
                    Console.WriteLine($"File dequeued: {filePath}");
                    try
                    {
                        // Process file
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<StatementContext>();
                            var statementParser = scope.ServiceProvider.GetRequiredService<StatementParser>();

                            if (dbContext == null)
                            {
                                throw new ArgumentNullException(nameof(dbContext));
                            }

                            // Use the statementParser to process the file
                            var account = statementParser.AccountParser(filePath);

                            // Check if the account exists
                            var existingAccount = await dbContext._Accounts
                                .Include(a => a.Transactions) // Include related transactions for comparison
                                .FirstOrDefaultAsync(a => a.AccountNumber == account.AccountNumber);

                            if (existingAccount == null)
                            {
                                // Account doesn't exist, add it along with its transactions
                                dbContext._Accounts.Add(account);
                            }
                            else
                            {
                                // Account exists, update its details
                                existingAccount.Name = account.Name;
                                existingAccount.Address = account.Address;
                                existingAccount.StatementDate = account.StatementDate;
                                existingAccount.FromDate = account.FromDate;
                                existingAccount.ToDate = account.ToDate;
                                existingAccount.OpeningBalance = account.OpeningBalance;
                                existingAccount.ClosingBalance = account.ClosingBalance;
                                existingAccount.Balance = account.ClosingBalance;
                                existingAccount.TransactionCount = account.TransactionCount;

                                // Add only new transactions
                                var newTransactions = account.Transactions
                                    .Where(t => !existingAccount.Transactions
                                        .Any(et => et.Date == t.Date && et.Description == t.Description && et.Amount == t.Amount));

                                dbContext._Transactions.AddRange(newTransactions);
                            }
                            await dbContext.SaveChangesAsync(); //save
                        }

                    }
                    catch (Exception ex)
                    {
                        // Log error and continue processing
                        Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    }
                }
            }
        }

        // Service starts
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StatementProcessingService is starting.");
            return base.StartAsync(cancellationToken);
        }

        // Service stops
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StatementProcessingService is stopping.");
            return base.StopAsync(cancellationToken);
        }

        // Dispose called
    }
}
