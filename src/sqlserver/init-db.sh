#!/bin/bash
# Wait for SQL Server to be ready
sleep 5

# Create NServiceBus database if it doesn't exist
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -C -Q "IF DB_ID('NServiceBus') IS NULL CREATE DATABASE [NServiceBus];"

echo "Database initialization complete"
