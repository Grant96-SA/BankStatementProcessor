using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace BankProcessor.API.Transaction
{
    public record GetTransactionsRequest(string Uuid);
}