# Local SQL Server in Docker

The application now uses SQL Server 2022 Developer in the dedicated `cardflow-sqlserver` container.
This edition is for development and testing. Other existing containers are not modified.

## Start and stop

Run these commands from the repository root:

```powershell
docker compose up -d --wait
docker compose stop
```

Data is stored in the named volume `cardflow_cardflow-sql-data`. Do not use `docker compose down -v`
unless you intend to delete that database permanently.

## Connect with SSMS

- Server: `127.0.0.1,14333`
- Authentication: SQL Server Authentication
- Login: `sa`
- Password: the `CARDFLOW_SQL_PASSWORD` value in the local `.env` file
- Database: `CardFlow`
- Trust server certificate: enabled (the local container uses a development certificate)

The port is bound to localhost only. `.env` and `appsettings.secrets.json` are ignored by Git.
The host and migrator load the local connection string from their `appsettings.secrets.json` files.
Do not publish these files or use the development `sa` account for production deployments.

## Schema and initial data

```powershell
dotnet run --project src/Learn.CardFlow.DbMigrator
```

The migrator applies the SQL Server schema and seeds ABP's initial admin account, permissions,
and OpenIddict clients. Newly created users and roles must be managed in the application.

For future migrations, work in `src/Learn.CardFlow.EntityFrameworkCore`:

```powershell
dotnet ef migrations add YourChange --output-dir Migrations/SqlServer
```

The old SQLite database `CardFlow.db` is preserved. Its root-level migration files are kept
as a reference and excluded from compilation. The active SQL Server migrations are under
`Migrations/SqlServer`. Existing SQLite users, roles, grants, and requests were not copied
into the new SQL Server database.

Tests can continue to use their own isolated in-memory SQLite database.

Image and container setup follow Microsoft's SQL Server container documentation:
https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker?view=sql-server-ver16

After switching databases, sign out and sign in again so old authentication sessions are not
reused against the new database. The former Reviewer user and role remain in SQLite only.
