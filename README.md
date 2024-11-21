# Bank Processor

## Overview

The **Bank Processor** is designed to process and manage bank statements and transactions efficiently. It provides APIs for uploading bank statements, retrieving account details, and querying transaction histories. The application is built with .NET and utilizes Entity Framework Core for database operations.

---

## Features

- **Bank Statement Upload**: Upload PDF bank statements and process them asynchronously in the background using a hosted service.
- **Account Management**: Query account details, including balances, opening/closing balances, and transaction summaries.
- **Transaction Processing**: Parse and store transactions from uploaded statements into the database.
- **Background Processing**: Utilize a background service to process statements while maintaining responsiveness.
- **RESTful API**: Integrate with third-party systems using the exposed API endpoints.

---

## Technologies Used

- **.NET 9**: For building the web application and server functionality.
- **Entity Framework Core**: For database interactions.
- **iText7**: For extracting data from PDF statements.
- **SQLite**: Lightweight database for storage.
- **ASP.NET Core**: For creating RESTful API endpoints.

---

## Prerequisites

- **.NET SDK**: Version 6.0 or higher (this guide uses version 9).
- **SQLite**: Pre-installed on your system, or another database with connection strings updated accordingly.
- **IDE**: Visual Studio, Visual Studio Code, or another IDE that supports .NET development.
- **REST Client**: Use the REST Client extension in VS Code (Huachao Mao) or cURL for testing.

---

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/Grant96-SA/BankStatementProcessor
cd BankStatementProcessor
