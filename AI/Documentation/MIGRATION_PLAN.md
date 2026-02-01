﻿﻿### Phase 5: Integration & Testing
- Complete docker-compose.local.yml with all services
- Update ServiceControl/ServicePulse configurations
- Test complete message flow
- Verify monitoring (Prometheus, Grafana, Jaeger)
- Document setup and usage
### Phase 4: Credit Bureau Migration
- Create Azure Functions project (.NET 10 Isolated)
- Port JavaScript logic to C#
- Create Dockerfile using Azure Functions base image
- Add to docker-compose
- Update LoanBroker HTTP client
### Phase 3: SQL Server Persistence
- Replace `NServiceBus.Persistence.DynamoDB` with `NServiceBus.Persistence.Sql`
- Add SQL Server 2022 container to docker-compose
- Configure SQL persistence in SharedConventions
- Enable installers for schema creation
- Update connection strings
### Phase 2: Azure Service Bus Emulator
- Replace `NServiceBus.AmazonSQS` with `NServiceBus.Transport.AzureServiceBus`
- Create `servicebus-config.json` for emulator
- Add Service Bus Emulator container to docker-compose
- Update connection strings to point to emulator
- Update endpoint configurations
### Phase 1: .NET 10 Upgrade
- Update `global.json` to .NET 10 SDK
- Update all `.csproj` files to `net10.0`
- Update all Dockerfiles to .NET 10 base images
- Update NuGet packages
- Run tests
# Azure Loan Broker Migration Plan

**Version:** 2.0  
**Date:** January 30, 2026
**Status:** Planning
**Status:** Planning

---

## Overview

Migration of the AWS LoanBroker example to use Azure-equivalent services running locally in Docker containers.

### Goals

1. **Upgrade to .NET 10** - All projects from .NET 8 → .NET 10
2. **AWS → Azure Services** - Replace AWS services with Azure local equivalents
3. **Local-First** - Everything runs in Docker containers locally (no cloud required)

---

## Services Mapping

| AWS Service | Azure Equivalent | Implementation |
|-------------|------------------|----------------|
| **SQS** | Azure Service Bus | Service Bus Emulator (Docker) |
| **DynamoDB** | SQL Server | SQL Server 2022 (Docker) |
| **Lambda** | Azure Functions | Azure Functions (Docker) |
| **LocalStack** | Native emulators | Service Bus Emulator + Azurite |
| **CloudWatch** | Seq | Seq (Docker) |
| **X-Ray** | Jaeger | Existing container (keep) |

### NServiceBus Changes

- **Transport:** `NServiceBus.AmazonSQS` → `NServiceBus.Transport.AzureServiceBus`
- **Persistence:** `NServiceBus.Persistence.DynamoDB` → `NServiceBus.Persistence.Sql`

### Application Components (No Changes)

- Client - Sends loan requests
- LoanBroker - Orchestration service with sagas
- Bank1Adapter, Bank2Adapter, Bank3Adapter - Bank integration
- EmailSender - Customer notifications
- Credit Bureau - Credit score provider (JavaScript → C# Azure Function)

---

## Migration Phases

### Phase 1: .NET 10 Upgrade ✅
- ✅ Update `global.json` to .NET 10 SDK
- ✅ Update all `.csproj` files to `net10.0`
- ✅ Update all Dockerfiles to .NET 10 base images
- ✅ Update NuGet packages
- ✅ Run tests

### Phase 2: Azure Service Bus Emulator ✅
- ✅ Replace `NServiceBus.AmazonSQS` with `NServiceBus.Transport.AzureServiceBus`
- ✅ Create `servicebus-config.json` for emulator
- ✅ Add Service Bus Emulator container to docker-compose
- ✅ Update connection strings to point to emulator
- ✅ Update endpoint configurations

### Phase 3: SQL Server Persistence ✅
- ✅ Replace `NServiceBus.Persistence.DynamoDB` with `NServiceBus.Persistence.Sql`
- ✅ Add SQL Server 2022 container to docker-compose
- ✅ Configure SQL persistence in SharedConventions
- ✅ Enable installers for schema creation
- ✅ Update connection strings

### Phase 4: Credit Bureau Migration ✅
- ✅ Create Azure Functions project (.NET 10 Isolated)
- ✅ Port JavaScript logic to C#
- ✅ Create Dockerfile using Azure Functions base image
- ✅ Add to docker-compose
- ✅ Update LoanBroker HTTP client

### Phase 5: Integration & Testing
- Complete docker-compose.local.yml with all services
- Update ServiceControl/ServicePulse configurations
- Test complete message flow
- Verify monitoring (Prometheus, Grafana, Jaeger)
- Document setup and usage

---

## Key Deliverables

### Modified Files
- `global.json` - .NET 10 SDK version
- All `.csproj` files - Target framework
- All `Dockerfiles` - .NET 10 base images  
- `CommonConfigurations/SharedConventions.cs` - Transport and persistence config
- All `appsettings.json` - Connection strings

### New Files
- `docker-compose.local.yml` - Complete local stack
- `servicebus-config.json` - Service Bus Emulator config
- `.env.local` - Local environment variables
- `src/CreditBureau/` - Azure Functions project
- Helper scripts (start/stop/reset)

### Removed Files
- `lambdas/creditbureau.js`
- `localstack/init-aws.sh`
- `env/aws.env`

---

## Prerequisites

- .NET 10 SDK
- Docker Desktop (8GB+ RAM)
- Docker Compose v2+
- Available ports: 1433, 5672, 10000-10002, 5100, 5341, 16686, 33333, 9999

---

## Success Criteria

- All services run on .NET 10
- `docker-compose up` starts entire system
- Azure Service Bus Emulator handles all messaging
- SQL Server handles all persistence
- Complete loan flow works end-to-end
- All monitoring tools functional
- Zero cloud costs

---

## References

- [Azure Service Bus Emulator](https://learn.microsoft.com/azure/service-bus-messaging/overview-emulator)
- [NServiceBus Azure Service Bus Transport](https://docs.particular.net/transports/azure-service-bus/)
- [NServiceBus SQL Persistence](https://docs.particular.net/persistence/sql/)
- [Azure Functions in Containers](https://learn.microsoft.com/azure/azure-functions/functions-create-function-linux-custom-image)

