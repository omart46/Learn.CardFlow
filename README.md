# Learn.CardFlow

An educational employee card management system built with ABP, ASP.NET Core, Angular, and SQL Server.

## Features

- Card enrollment, review, approval, rejection, correction, and resubmission.
- Production batches and production tracking.
- Quality inspections with pass/fail results and failure reasons.
- Card handover with recipient, time, and delivery officer recorded.
- Dashboard totals scoped to the current user's permissions.
- Request search by employee name/number, status, and creation date.
- Separate reviewer, supervisor, production, quality, and delivery permissions.

Reproduction of failed cards is not implemented yet. The account switcher lists local demonstration accounts; it uses real sign-out/sign-in and does not store passwords. These accounts are not automatically created in a new database.

## Requirements

- .NET 10 SDK
- Node.js compatible with Angular 21 (this workspace was tested with Node 24)
- Docker Desktop with Linux containers
- ABP CLI for restoring host UI libraries

## Local setup

1. Copy `.env.example` to `.env` and set a strong `CARDFLOW_SQL_PASSWORD`.
2. Start SQL Server:

   ```powershell
   docker compose up -d --wait
   ```

3. Create `appsettings.secrets.json` in both `src/Learn.CardFlow.HttpApi.Host` and `src/Learn.CardFlow.DbMigrator` with your local connection string:

   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=tcp:127.0.0.1,14333;Database=CardFlow;User Id=sa;Password=YOUR_LOCAL_PASSWORD;Encrypt=True;TrustServerCertificate=True;"
     }
   }
   ```

   Use the same password as `.env`. In the host secrets file, also set a private `StringEncryption:DefaultPassPhrase` for your installation. Existing installations must retain their original passphrase.

4. Restore dependencies and prepare the database:

   ```powershell
   dotnet restore Learn.CardFlow.slnx
   abp install-libs
   dotnet dev-certs https --trust
   dotnet run --project src/Learn.CardFlow.DbMigrator
   ```

5. Run the API:

   ```powershell
   dotnet run --project src/Learn.CardFlow.HttpApi.Host
   ```

6. In another terminal, run Angular:

   ```powershell
   cd angular
   npm ci
   npm start
   ```

- Frontend: http://localhost:4200
- Swagger: https://localhost:44367/swagger

Configure roles and grants in Identity Management. Operational roles should not be Default. Use a production certificate and private configuration before deploying; development certificates and credentials are not included.

## Structure

- `src/Learn.CardFlow.Domain.Shared`: enums and localization.
- `src/Learn.CardFlow.Domain`: entities and business rules.
- `src/Learn.CardFlow.Application.Contracts`: DTOs, service interfaces, and permissions.
- `src/Learn.CardFlow.Application`: application services.
- `src/Learn.CardFlow.EntityFrameworkCore`: persistence and active SQL Server migrations in `Migrations/SqlServer`.
- `angular`: frontend.
- `test`: domain and integration tests, using isolated SQLite databases.

## Verification

```powershell
dotnet build Learn.CardFlow.slnx
dotnet test test/Learn.CardFlow.Domain.Tests
dotnet test test/Learn.CardFlow.EntityFrameworkCore.Tests
cd angular
npm run build -- --configuration development
```

Local databases, certificates, passwords, logs, dependency folders, and build output are excluded from Git. Cloning this repository does not copy existing users or employee data.
