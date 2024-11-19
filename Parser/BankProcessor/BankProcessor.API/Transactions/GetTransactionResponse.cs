namespace BankProcessor.API.Transaction
{
    public record GetTransactionsResponse
    (
        string Uuid,                // Statement UUID
        string Message,             // Response message
        List<TransactionData> Data  // List of transactions
    );

    public record TransactionData
    (
        string Uuid,                // Transaction UUID
        DateTime Date,              // Transaction date
        string Description,         // Transaction description
        decimal Amount,             // Transaction amount
        decimal Balance,            // Balance after the transaction
        string Type                 // Type of transaction: credit or debit
    );
}