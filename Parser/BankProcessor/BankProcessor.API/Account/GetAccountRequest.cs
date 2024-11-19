using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace BankProcessor.API.Account
{
    public record GetAccountRequest(string Uuid);
}