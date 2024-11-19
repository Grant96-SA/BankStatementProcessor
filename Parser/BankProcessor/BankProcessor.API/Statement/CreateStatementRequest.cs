using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace BankProcessor.API.Statement
{
    public record CreateStatementRequest
    (
        Payload payload

    );

    public record Payload
    (
        IFormFile? Statement
    );
}
