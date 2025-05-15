# ProjectName: .NET 9 CQRS Prototype

## Overview

This project is a prototype .NET 9 backend application demonstrating a clean architecture approach with CQRS,
event-driven principles using Azure Service Bus, and reliable event publishing via the Transactional Outbox pattern.

The primary goal is to showcase ACID compliance across multiple database operations triggered by a single initial
command, with events only being published to the bus after the successful completion of the encompassing database
transaction.

## Prerequisites

- .NET 9 SDK
- MSSQL Server (or SQL Server Express/Developer Edition)
- Azure Service Bus (or emulator, e.g., Azure Functions Core Tools with local storage emulator)
- (Add any other specific tools or SDKs required)

## Build Instructions

```bash
# Navigate to the solution directory
cd path/to/solution

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

## Running the Application

1. **Configure Connection Strings:**
    - Update `appsettings.json` (or `appsettings.Development.json`) in `ProjectName.Api` with your MSSQL Server
      connection string.
    - Update configuration with your Azure Service Bus connection string and queue/topic name.
2. **Apply EF Core Migrations:**

    ```bash
    # Navigate to the Infrastructure project directory
    cd path/to/ProjectName.Infrastructure

    # Apply migrations
    dotnet ef database update --startup-project ../ProjectName.Api
    ```

3. **Run the API:**

    ```bash
    # Navigate to the API project directory
    cd path/to/ProjectName.Api

    # Run the application
    dotnet run
    ```

   The API will typically be available at `https://localhost:port` or `http://localhost:port`.

## Testing

(Details on how to run tests - see `TESTING_STRATEGY.md` for more info)

```bash
# Navigate to the solution directory or test project directory
# dotnet test
```

## Demo Goals

This prototype aims to demonstrate:

- Implementation of CQRS with MediatR.
- Transactional outbox pattern for reliable event publishing.
- ACID compliance for commands involving multiple database operations.
- Eventual consistency via Azure Service Bus.
- Clean architecture principles in a .NET 9 application.
