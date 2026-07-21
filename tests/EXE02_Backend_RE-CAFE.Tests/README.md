# Payment management integration tests

This project exercises the payment-management API through the real ASP.NET Core HTTP pipeline and a real PostgreSQL database.

## Prerequisites

- .NET 10 SDK
- A local PostgreSQL instance reachable with the connection in `appsettings.Development.json`, or an explicit admin connection in `PAYMENT_TEST_POSTGRES_ADMIN`
- The database account must be allowed to create and drop isolated test databases

The fixture creates a database named `recafe_payment_tests_<guid>`, applies all migrations, runs the suite, and drops that exact generated database. Automatic discovery from the development settings is restricted to localhost. Use the environment variable deliberately when targeting another PostgreSQL host.

## Run

```powershell
dotnet test tests\EXE02_Backend_RE-CAFE.Tests\EXE02_Backend_RE-CAFE.Tests.csproj -c Release
```

The suite covers role authorization, list/detail/summary/export contracts, combined filters, CSV safety, audit events, migration/index presence, COD and bank-transfer checkout, profile retrieval of an unpaid order, SePay webhook idempotency and underpayment handling. Its performance case seeds 10,001 payments and requires the 95th percentile list latency at page size 100 to remain below 500 ms.
