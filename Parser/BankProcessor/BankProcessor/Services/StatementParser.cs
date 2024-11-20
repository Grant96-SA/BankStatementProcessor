using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using BankProcessor.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
                FromDate = ParseDate(GetValue(pdfContent, "From Date: ", "\n")),
                ToDate = ParseDate(GetValue(pdfContent, "To Date: ", "\n")),
                Transactions = ParseTransactions(pdfContent, GetValue(pdfContent, "Account Number: ", "\n"))
            };

            return account;
        }

        // Extract text from the pdf
        private string ExtractTextFromPdf(string filePath)
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);

            var text = new StringBuilder();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i)));
            }
            return text.ToString();
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
            if (DateOnly.TryParseExact(dateText, "MMMM dd, yyyy", CultureInfo.InvariantCulture,DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateOnly.MinValue; // Return a default value if parsing fails
        }

        private List<Transaction> ParseTransactions(string pdfContent, string accountNumber)
        {
            var transactions = new List<Transaction>();

            // Match lines containing transaction details
            var transactionRegex = new Regex(@"^(\d{4}-\d{2}-\d{2})\s+(.*?)\s+(\d+\.\d+|\s+)?\s+(\d+\.\d+|\s+)?\s+(\d+\.\d+)$", RegexOptions.Multiline);
            var matches = transactionRegex.Matches(pdfContent);

            foreach (Match match in matches)
            {
                // Extract columns
                var date = match.Groups[1].Value;
                var description = match.Groups[2].Value.Trim();
                var credit = match.Groups[3].Value.Trim();
                var debit = match.Groups[4].Value.Trim();
                var balance = match.Groups[5].Value.Trim();

                // Determine transaction type by checking if Credit column entry empty
                var type = !string.IsNullOrEmpty(credit) ? "Credit" : "Debit";

                transactions.Add(new Transaction
                {
                    Date = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Description = description,
                    Amount = decimal.Parse(!string.IsNullOrEmpty(credit) ? credit : debit),
                    Balance = decimal.Parse(balance),
                    Type = type,
                    AccountNumber = accountNumber
                });
            }
            return transactions;
        }
    }
}
