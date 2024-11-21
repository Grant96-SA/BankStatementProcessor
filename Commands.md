## Package installation
- dotnet add package iText7
- dotnet add package Microsoft.AspNetCore.OpenApi
- dotnet add package Microsoft.EntityFrameworkCore
- dotnet add package Microsoft.EntityFrameworkCore.Sqlite
- dotnet add package Microsoft.EntityFrameworkCore.Tools
- dotnet add package System.Text.Encodings.Web
- dotnet add package Microsoft.AspNetCore.Http

## Build and run
-> navigate to head BankProcessor folder
-dotnet build

-> Run program.cs in lower BankProcessor folder
-dotnet run --project ./BankProcessor/

--> application will be available on http://localhost:5119

## Testing

## POST
-curl -X POST http://localhost:5119/statement \
     -H "Content-Type: multipart/form-data" \
     -F "payload.Statement=@./data/fundingbank_1.pdf;type=application/pdf"

## GET Account
-curl -X GET http://localhost:5119/account/2E04A15D-B23D-4785-8CF7-4D99C87004C9

## GET Transactions
-curl -X GET http://localhost:5119/transactions/2E04A15D-B23D-4785-8CF7-4D99C87004C9

## Pre-stored UUIDs
-John Doe	2E04A15D-B23D-4785-8CF7-4D99C87004C9
-Jane Smith	B83A3DAE-ED2B-4028-A159-59AF19D72E39

