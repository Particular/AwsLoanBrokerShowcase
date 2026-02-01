# Azure Loan Broker - Quick Start Script (PowerShell)
# This script starts all services in the correct order with health checks

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Azure Loan Broker System..." -ForegroundColor Cyan
Write-Host ""

function Wait-ForService {
    param(
        [string]$ServiceName,
        [int]$MaxAttempts = 30
    )
    
    Write-Host "⏳ Waiting for $ServiceName to be ready..." -NoNewline
    
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        $status = docker-compose ps $ServiceName 2>$null
        if ($status -match "Up") {
            Write-Host " ✓" -ForegroundColor Green
            return $true
        }
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
    }
    
    Write-Host " ⚠ Timeout" -ForegroundColor Yellow
    return $false
}

function Wait-ForHttp {
    param(
        [string]$Url,
        [int]$MaxAttempts = 30
    )
    
    Write-Host "⏳ Waiting for $Url..." -NoNewline
    
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host " ✓" -ForegroundColor Green
                return $true
            }
        }
        catch {
            # Ignore errors
        }
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
    }
    
    Write-Host " ⚠ Timeout" -ForegroundColor Yellow
    return $false
}

Write-Host "Step 1/6: Starting Core Infrastructure" -ForegroundColor Cyan
Write-Host "---------------------------------------"
docker-compose up -d servicebus-emulator sqlserver creditbureau servicecontrol-db

Wait-ForService "servicebus-emulator" 30
Wait-ForService "sqlserver" 30
Wait-ForService "creditbureau" 30
Wait-ForService "servicecontrol-db" 30

Write-Host ""
Write-Host "Step 2/6: Starting ServiceControl Stack" -ForegroundColor Cyan
Write-Host "---------------------------------------"
docker-compose up -d servicecontrol servicecontrol-audit servicecontrol-monitoring

Wait-ForService "servicecontrol" 60
Wait-ForService "servicecontrol-audit" 30
Wait-ForService "servicecontrol-monitoring" 30

Write-Host ""
Write-Host "Step 3/6: Starting ServicePulse" -ForegroundColor Cyan
Write-Host "---------------------------------------"
docker-compose up -d servicepulse

Wait-ForService "servicepulse" 30
Wait-ForHttp "http://localhost:9999" 30

Write-Host ""
Write-Host "Step 4/6: Starting Monitoring Stack" -ForegroundColor Cyan
Write-Host "---------------------------------------"
docker-compose up -d prometheus grafana jaeger adot

Wait-ForService "prometheus" 30
Wait-ForService "grafana" 30
Wait-ForService "jaeger" 30

Write-Host ""
Write-Host "Step 5/6: Starting Application Services" -ForegroundColor Cyan
Write-Host "---------------------------------------"
docker-compose up -d loan-broker bank1 bank2 bank3 email-sender

Wait-ForService "loan-broker" 60
Wait-ForService "bank1" 30
Wait-ForService "bank2" 30
Wait-ForService "bank3" 30
Wait-ForService "email-sender" 30

Write-Host ""
Write-Host "Step 6/6: System Ready" -ForegroundColor Cyan
Write-Host "---------------------------------------"
Write-Host ""
Write-Host "✅ Azure Loan Broker System is Ready!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Access Points:"
Write-Host "  • ServicePulse:  http://localhost:9999"
Write-Host "  • Jaeger:        http://localhost:16686"
Write-Host "  • Grafana:       http://localhost:3000 (admin/admin)"
Write-Host "  • Prometheus:    http://localhost:9090"
Write-Host ""
Write-Host "🎯 To start the demo client:"
Write-Host "  docker-compose up client"
Write-Host ""
Write-Host "📝 To view logs:"
Write-Host "  docker-compose logs -f loan-broker"
Write-Host ""
Write-Host "🛑 To stop all services:"
Write-Host "  docker-compose down"
Write-Host ""
