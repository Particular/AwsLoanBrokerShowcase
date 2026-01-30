# GitHub Copilot Instructions for Azure Loan Broker Showcase

## Project Context

This is a loan broker example application demonstrating NServiceBus messaging patterns using Azure services running locally in Docker containers. The project is being migrated from AWS services to Azure equivalents.

**IMPORTANT:** The existing project structure, service boundaries, and message flows are preserved. Only the underlying infrastructure (transport and persistence) has been migrated from AWS to Azure.

## Technology Stack

- **.NET Version:** .NET 10
- **Messaging:** NServiceBus with Azure Service Bus Emulator
- **Persistence:** NServiceBus.Persistence.Sql with SQL Server 2022
- **Containerization:** Docker with docker-compose
- **Monitoring:** Prometheus, Grafana, Jaeger
- **Service Management:** ServiceControl, ServicePulse

## Code Style Guidelines

### General C# Conventions

- Use C# 14 features where appropriate
- Prefer `required` properties over constructor parameters for DTOs
- Use top-level statements for Program.cs files
- Use file-scoped namespaces
- Prefer `var` when type is obvious
- Use nullable reference types (`#nullable enable`)

### Dockerfile Standards

- Use .NET 10 base images: `mcr.microsoft.com/dotnet/aspnet:10.0` and `mcr.microsoft.com/dotnet/sdk:10.0`
- Always use multi-stage builds (build → runtime)
- Set `DOTNET_EnableDiagnostics=0` for production images
- Expose appropriate ports and document them

### Configuration Files

- Use `appsettings.json` for base configuration
- Override with `appsettings.Development.json` for local settings
- Connection strings should support both local Docker and production Azure
- Never hardcode sensitive values (use environment variables)

## Dependencies to Avoid

- ❌ AWS SDK packages (AWSSDK.*)
- ❌ NServiceBus.AmazonSQS
- ❌ NServiceBus.Persistence.DynamoDB
- ❌ LocalStack references

## Dependencies to Use

- ✅ NServiceBus (v10.x)
- ✅ NServiceBus.Transport.AzureServiceBus
- ✅ NServiceBus.Persistence.Sql
- ✅ Microsoft.Data.SqlClient
- ✅ Azure.Messaging.ServiceBus (as needed)

## Docker Compose Guidelines

- All services should be defined in `docker-compose.local.yml`
- Use named volumes for persistent data
- Set appropriate health checks
- Define resource limits (memory/CPU)
- Use networks to isolate service groups
- Include depends_on with conditions for startup ordering

## Testing Approach

- Unit tests for message handlers
- Integration tests using NServiceBus.Testing
- End-to-end tests via docker-compose
- Always test saga timeout and completion scenarios

## Documentation Standards

- Update README.md when adding new services
- Document environment variables in .env.example
- Keep MIGRATION_PLAN.md updated with progress
- Add inline comments for complex NServiceBus configurations

## Common Pitfalls to Avoid

- Don't use synchronous I/O in message handlers
- Don't create circular message dependencies
- Don't share saga data instances between messages
- Don't forget to enable installers in development (`endpointConfig.EnableInstallers()`)
- Don't use blocking calls in async handlers

## Migration Constraints

### What Changed (Infrastructure Only)
- ✅ Transport: AWS SQS → Azure Service Bus Emulator
- ✅ Persistence: DynamoDB → SQL Server with NServiceBus.Persistence.Sql
- ✅ .NET Version: .NET 8 → .NET 10
- ✅ Container images: Updated base images
- ✅ CreditBureau: Lambda (JavaScript) → Azure Function (C#)

### What Did NOT Change (Preserve These)
- ❌ Service names and responsibilities
- ❌ Project structure and folder organization (except CreditBureau conversion)
- ❌ Message types and contracts
- ❌ Saga orchestration logic
- ❌ Business logic in handlers
- ❌ Endpoint names and routing
- ❌ Message flow patterns

## When Suggesting Code Changes

- **DO NOT** suggest creating new projects or folders
- **DO NOT** suggest moving files between projects
- **DO NOT** suggest renaming services or endpoints
- **DO NOT** suggest changing message types or contracts
- **DO NOT** suggest restructuring the solution
- **DO** focus on infrastructure configuration changes within existing files
- **DO** update transport and persistence code in existing endpoints
- **DO** update connection strings and configurations
- **DO** update NuGet package references
- **DO** update Dockerfiles and docker-compose entries
