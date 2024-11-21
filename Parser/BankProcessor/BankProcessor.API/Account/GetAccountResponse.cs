namespace BankProcessor.API.Account
{
    public record GetAccountResponse
    (
        Guid Uuid,                // Statement UUID
        string Message,             // Response message
        AccountData? Data            // Account details
    );

    public record AccountData
    (
        string Name,                // Account holder's name
        string Address,             // Account holder's address
        AccountDetails Account      // Nested account details
    );

    public record AccountDetails
    (
        Guid Uuid,                // Account UUID
        string Number,              // Account number
        decimal Balance,            // Current balance
        decimal OpeningBalance,     // Opening balance
        decimal ClosingBalance,     // Closing balance
        DateOnly FromDate,          // Statement start date
        DateOnly ToDate,            // Statement end date
        int TransactionCount,       // Number of transactions
        DateOnly StatementDate      // Statement generation date
    );
}