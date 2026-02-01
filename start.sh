#!/bin/bash

# Azure Loan Broker - Quick Start Script
# This script starts all services in the correct order with health checks

set -e

echo "🚀 Starting Azure Loan Broker System..."
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to wait for service
wait_for_service() {
    local service=$1
    local max_attempts=$2
    local attempt=1
    
    echo -n "⏳ Waiting for $service to be ready..."
    
    while [ $attempt -le $max_attempts ]; do
        if docker-compose ps $service | grep -q "Up"; then
            echo -e " ${GREEN}✓${NC}"
            return 0
        fi
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    echo -e " ${YELLOW}⚠ Timeout${NC}"
    return 1
}

# Function to check HTTP endpoint
wait_for_http() {
    local url=$1
    local max_attempts=$2
    local attempt=1
    
    echo -n "⏳ Waiting for $url..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -sf "$url" > /dev/null 2>&1; then
            echo -e " ${GREEN}✓${NC}"
            return 0
        fi
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    echo -e " ${YELLOW}⚠ Timeout${NC}"
    return 1
}

echo "Step 1/6: Starting Core Infrastructure"
echo "---------------------------------------"
docker-compose up -d servicebus-emulator sqlserver creditbureau servicecontrol-db

wait_for_service "servicebus-emulator" 30
wait_for_service "sqlserver" 30
wait_for_service "creditbureau" 30
wait_for_service "servicecontrol-db" 30

echo ""
echo "Step 2/6: Starting ServiceControl Stack"
echo "---------------------------------------"
docker-compose up -d servicecontrol servicecontrol-audit servicecontrol-monitoring

wait_for_service "servicecontrol" 60
wait_for_service "servicecontrol-audit" 30
wait_for_service "servicecontrol-monitoring" 30

echo ""
echo "Step 3/6: Starting ServicePulse"
echo "---------------------------------------"
docker-compose up -d servicepulse

wait_for_service "servicepulse" 30
wait_for_http "http://localhost:9999" 30

echo ""
echo "Step 4/6: Starting Monitoring Stack"
echo "---------------------------------------"
docker-compose up -d prometheus grafana jaeger adot

wait_for_service "prometheus" 30
wait_for_service "grafana" 30
wait_for_service "jaeger" 30

echo ""
echo "Step 5/6: Starting Application Services"
echo "---------------------------------------"
docker-compose up -d loan-broker bank1 bank2 bank3 email-sender

wait_for_service "loan-broker" 60
wait_for_service "bank1" 30
wait_for_service "bank2" 30
wait_for_service "bank3" 30
wait_for_service "email-sender" 30

echo ""
echo "Step 6/6: System Ready"
echo "---------------------------------------"
echo ""
echo -e "${GREEN}✅ Azure Loan Broker System is Ready!${NC}"
echo ""
echo "📊 Access Points:"
echo "  • ServicePulse:  http://localhost:9999"
echo "  • Jaeger:        http://localhost:16686"
echo "  • Grafana:       http://localhost:3000 (admin/admin)"
echo "  • Prometheus:    http://localhost:9090"
echo ""
echo "🎯 To start the demo client:"
echo "  docker-compose up client"
echo ""
echo "📝 To view logs:"
echo "  docker-compose logs -f loan-broker"
echo ""
echo "🛑 To stop all services:"
echo "  docker-compose down"
echo ""
