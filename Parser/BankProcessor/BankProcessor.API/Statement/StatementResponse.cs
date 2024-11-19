namespace BankProcessor.API.Statement
{
    public record CreateStatementResponse
    (
        Guid uuid,
        string message
    );
}