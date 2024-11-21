using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using BankProcessor.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace BankProcessor.Services
{
    public class StatementParser
    {

        public Account AccountParser(string filePath)
        {
            string pdfContent = ExtractTextFromPdf(filePath);

            // Parse account details
            var account = new Account
            {
                Name = GetValue(pdfContent, "Account Holder: ", "\n"),
                AccountNumber = GetValue(pdfContent, "Account Number: ", "\n"),
                Address = GetValue(pdfContent, "Address: ", "\n"),
                StatementDate = ParseDate(GetValue(pdfContent, "Statement Date: ", "\n")),
            };

            // Parse transactions and associate them with the account
            var transactions = ParseTransactions(pdfContent, account.AccountNumber);

            // Compute account summary from transactions
            if (transactions.Any())
            {
                account.FromDate = transactions.Min(t => t.Date); // Earliest transaction date
                account.ToDate = transactions.Max(t => t.Date);   // Latest transaction date
                account.OpeningBalance = transactions.First().Balance;
                account.ClosingBalance = transactions.Last().Balance;
                account.TransactionCount = transactions.Count;  // Total number of transactions
                account.Balance = account.ClosingBalance;      // Current balance matches closing balance
            }
            else
            {
                // Default values if no transactions are found
                account.FromDate = DateOnly.MinValue;
                account.ToDate = DateOnly.MinValue;
                account.OpeningBalance = 0;
                account.ClosingBalance = 0;
                account.TransactionCount = 0;
                account.Balance = 0;
            }

            account.Transactions = transactions;
            return account;

        }

        // Extract text from the pdf
        private string ExtractTextFromPdf(string filePath)
        {
            using var pdfReader = new PdfReader(filePath);
            {
                using var pdfDocument = new PdfDocument(pdfReader);
                {
                    var text = new StringBuilder();
                    for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                    {
                        var page = pdfDocument.GetPage(pageNumber);
                        var strategy = new SimpleTextExtractionStrategy();
                        var content = PdfTextExtractor.GetTextFromPage(page, strategy);
                        text.Append(content);
                    }
                    return text.ToString();
                }
            }
        }

        // Extract a value between two delimiters (e.g., a label and a new line)
        private string GetValue(string content, string startDelimiter, string endDelimiter)
        {
            // Get Starting index
            var startIndex = content.IndexOf(startDelimiter);
            if (startIndex == -1)
            {
                return string.Empty;
            }
            //Get End index
            startIndex += startDelimiter.Length;
            var endIndex = content.IndexOf(endDelimiter, startIndex);
            if (endIndex == -1)
            {
                return string.Empty;
            }

            return content.Substring(startIndex, endIndex - startIndex).Trim();
        }

        // Parse dates safely from the string
        private DateOnly ParseDate(string dateText)
        {
            if (DateOnly.TryParseExact(dateText, "MMMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateOnly.MinValue; // Return a default value if parsing fails
        }

        private List<Transaction> ParseTransactions(string PDFcontent, string accountNumber)
        {
            var transactions = new List<Transaction>();

            // Extract each line from PDF
            var lines = PDFcontent.Split('\n');

            // List to hold table lines
            var tableLines = new List<string>();

            //
            foreach (var line in lines)
            {
                // Trim white spaces before processing
                var trimmedLine = line.Trim();

                // Check if the line starts with a valid date in YYYY-MM-DD format
                if (trimmedLine.Length >= 10 && DateOnly.TryParseExact(trimmedLine.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    tableLines.Add(trimmedLine);
                }
            }

            foreach (var line in tableLines)
            {
                {
                    var trimmedLine = line.Trim();

                    // Regex patterns for date, credit, debit, and balance
                    var datePattern = @"\d{4}-\d{2}-\d{2}";      // Date: YYYY-MM-DD
                    var creditPattern = @"(\d+\.\d+)";            // Credit (decimal value)
                    var debitPattern = @"(\d+\.\d+)";             // Debit (decimal value)
                    var balancePattern = @"(\d+\.\d+)$";          // Balance (decimal value at the end)

                    // Find the date using the date regex
                    var dateMatch = Regex.Match(trimmedLine, datePattern);
                    if (!dateMatch.Success)
                    {
                        Console.WriteLine($"Skipping line with no valid date: {line}");
                        continue;
                    }

                    // Extract the date from the match
                    var date = dateMatch.Value;

                    // Find the credit value using the credit regex
                    var creditMatch = Regex.Match(trimmedLine, creditPattern);
                    var creditAmount = creditMatch.Success ? decimal.Parse(creditMatch.Value) : 0;

                    // Find the debit value using the debit regex
                    var debitMatch = Regex.Match(trimmedLine, debitPattern);
                    var debitAmount = debitMatch.Success ? decimal.Parse(debitMatch.Value) : 0;

                    // Find the balance value using the balance regex
                    var balanceMatch = Regex.Match(trimmedLine, balancePattern);
                    var balanceAmount = balanceMatch.Success ? decimal.Parse(balanceMatch.Value) : 0;

                    // Determine the transaction type based on credit/debit
                    var type = creditAmount > 0 ? "Credit" : debitAmount > 0 ? "Debit" : "Unknown";

                    // Extract description by removing date, credit, debit, and balance parts
                    var description = trimmedLine
                        .Replace(date, "")
                        .Replace(creditMatch.Value, "")
                        .Replace(debitMatch.Value, "")
                        .Replace(balanceMatch.Value, "")
                        .Trim();

                    // Add the transaction to the list
                    transactions.Add(new Transaction
                    {
                        Date = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Description = description,
                        Amount = creditAmount > 0 ? creditAmount : debitAmount,
                        Balance = balanceAmount,
                        Type = type,
                    });
                }
            }
            return transactions;
        }
    }
}
