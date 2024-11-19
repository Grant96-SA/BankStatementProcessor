namespace BankProcessor.API.Account
{
    public record GetAccountResponse
    (
        string Uuid,                // Statement UUID
        string Message,             // Response message
        AccountData Data            // Account details
    );

    public record AccountData
    (
        string Name,                // Account holder's name
        string Address,             // Account holder's address
        AccountDetails Account      // Nested account details
    );

    public record AccountDetails
    (
        string Uuid,                // Account UUID
        string Number,              // Account number
        decimal Balance,            // Current balance
        decimal OpeningBalance,     // Opening balance
        decimal ClosingBalance,     // Closing balance
        DateTime FromDate,          // Statement start date
        DateTime ToDate,            // Statement end date
        int TransactionCount,       // Number of transactions
        DateTime StatementDate      // Statement generation date
    );
}