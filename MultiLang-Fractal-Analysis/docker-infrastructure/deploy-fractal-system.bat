@echo off
setlocal EnableDelayedExpansion

REM =============================================================================
REM Fractal Trading System Deployment Script
REM 
REM This deploys the full distributed system - Kafka, Spark, monitoring, the works.
REM It takes a few minutes to start everything up, so grab some coffee.
REM =============================================================================

echo.
echo ================================================================================
echo  FRACTAL TRADING SYSTEM
echo  Starting up the full distributed setup - this might take a few minutes
echo ================================================================================
echo.

REM Check if Docker is running
echo [INFO] Checking Docker availability...
docker --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker is not installed or not running
    echo Please install Docker Desktop and ensure it's running
    exit /b 1
)

REM Check if Docker Compose is available
echo [INFO] Checking Docker Compose availability...
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker Compose is not available
    echo Please install Docker Compose
    exit /b 1
)

echo [SUCCESS] Docker environment validated
echo.

REM Create necessary directories
echo [INFO] Creating project directories...
if not exist "ssl" mkdir ssl
if not exist "dashboards" mkdir dashboards
if not exist "..\distributed" mkdir ..\distributed
if not exist "..\kafka-streams" mkdir ..\kafka-streams
if not exist "..\hft-simulator" mkdir ..\hft-simulator
if not exist "..\spark-analysis" mkdir ..\spark-analysis

REM Generate SSL certificates for HTTPS (development only)
echo [INFO] Generating SSL certificates for development...
if not exist "ssl\cert.pem" (
    echo [INFO] Creating self-signed SSL certificate...
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 ^
        -keyout ssl\key.pem -out ssl\cert.pem ^
        -subj "/C=US/ST=NY/L=NYC/O=FractalTrading/CN=localhost" >nul 2>&1
    if errorlevel 1 (
        echo [WARNING] OpenSSL not found, skipping SSL certificate generation
        echo [INFO] HTTPS will not be available, but HTTP will work fine
    ) else (
        echo [SUCCESS] SSL certificate generated
    )
)

REM Clean up any existing containers and volumes (optional)
set /p cleanup="Do you want to clean up existing containers and volumes? (y/N): "
if /i "!cleanup!"=="y" (
    echo [INFO] Cleaning up existing Docker resources...
    docker-compose down -v --remove-orphans >nul 2>&1
    docker system prune -f >nul 2>&1
    echo [SUCCESS] Cleanup completed
)

REM Pull latest images
echo [INFO] Pulling latest Docker images...
docker-compose pull

REM Build custom images
echo [INFO] Building custom application images...
docker-compose build

REM Deploy the complete stack
echo.
echo [INFO] Deploying distributed fractal analysis system...
echo [INFO] This will start the following services:
echo   - Zookeeper (Kafka coordination)
echo   - Kafka (Message streaming)
echo   - Kafka UI (Management interface)
echo   - Spark Master + 2 Workers (Distributed processing)
echo   - Redis (Low-latency cache)
echo   - Prometheus (Metrics collection)
echo   - Grafana (Visualization dashboards)
echo   - InfluxDB (Time-series database)
echo   - Nginx (Reverse proxy and load balancer)
echo   - Market Data Producer (High-frequency data simulation)
echo   - Pattern Consumer (Real-time fractal detection)
echo   - HFT Engine (Trading simulation)
echo.

echo [INFO] Starting infrastructure services first...
docker-compose up -d zookeeper kafka redis prometheus influxdb

REM Wait for core services to be healthy
echo [INFO] Waiting for core services to become healthy...
:wait_loop
timeout /t 10 /nobreak >nul
docker-compose ps --filter "health=healthy" | findstr /r "zookeeper.*healthy kafka.*healthy" >nul
if errorlevel 1 (
    echo [INFO] Still waiting for core services...
    goto wait_loop
)

echo [SUCCESS] Core services are healthy
echo.

REM Start monitoring and UI services
echo [INFO] Starting monitoring and UI services...
docker-compose up -d grafana kafka-ui spark-master spark-worker-1 spark-worker-2

REM Wait for Spark cluster to be ready
echo [INFO] Waiting for Spark cluster to initialize...
timeout /t 20 /nobreak >nul

REM Start application services
echo [INFO] Starting application services...
docker-compose up -d market-producer pattern-consumer hft-engine

REM Start reverse proxy
echo [INFO] Starting reverse proxy...
docker-compose up -d nginx

REM Wait for all services to stabilize
echo [INFO] Allowing services to stabilize...
timeout /t 15 /nobreak >nul

REM Display service status
echo.
echo [SUCCESS] Deployment completed! Checking service status...
echo.
docker-compose ps

REM Display access information
echo.
echo ================================================================================
echo  ACCESS INFORMATION
echo ================================================================================
echo.
echo Web Interfaces (via Nginx reverse proxy):
echo   Primary Dashboard    : http://localhost/
echo   Grafana Dashboards  : http://localhost/grafana/ (admin / fractal2024)
echo   Kafka Management UI  : http://localhost/kafka/
echo   Spark Master UI      : http://localhost/spark/
echo   Prometheus Metrics   : http://localhost/prometheus/
echo.
echo Direct Service Access:
echo   Grafana             : http://localhost:3000 (admin / fractal2024)
echo   Kafka UI            : http://localhost:8080
echo   Spark Master        : http://localhost:8081
echo   Spark Worker 1      : http://localhost:8082
echo   Spark Worker 2      : http://localhost:8083
echo   Prometheus          : http://localhost:9090
echo   InfluxDB            : http://localhost:8086
echo.
echo API Endpoints:
echo   Pattern Metrics     : http://localhost:8001/metrics
echo   HFT Engine Metrics  : http://localhost:8002/metrics
echo   Nginx Health Check  : http://localhost/health
echo.
echo Data Ports:
echo   Kafka Broker        : localhost:9092
echo   Redis Cache         : localhost:6379
echo   InfluxDB API        : localhost:8086
echo.

REM Check if services are responding
echo ================================================================================
echo  SERVICE HEALTH CHECKS
echo ================================================================================
echo.

REM Check Nginx health
echo [INFO] Checking Nginx health...
curl -s http://localhost/health >nul 2>&1
if errorlevel 1 (
    echo [WARNING] Nginx health check failed
) else (
    echo [SUCCESS] Nginx is responding
)

REM Check Grafana
echo [INFO] Checking Grafana availability...
curl -s http://localhost:3000/api/health >nul 2>&1
if errorlevel 1 (
    echo [WARNING] Grafana health check failed
) else (
    echo [SUCCESS] Grafana is responding
)

REM Check Prometheus
echo [INFO] Checking Prometheus availability...
curl -s http://localhost:9090/-/healthy >nul 2>&1
if errorlevel 1 (
    echo [WARNING] Prometheus health check failed
) else (
    echo [SUCCESS] Prometheus is responding
)

echo.
echo ================================================================================
echo  SYSTEM MONITORING
echo ================================================================================
echo.
echo Real-time System Metrics:
echo.

REM Display resource usage
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}" | head -15

echo.
echo ================================================================================
echo  NEXT STEPS
echo ================================================================================
echo.
echo 1. Open the main dashboard: http://localhost/
echo 2. Login to Grafana with: admin / fractal2024
echo 3. Explore the "Fractal HFT Trading Dashboard"
echo 4. Monitor Kafka topics in the Kafka UI
echo 5. Check Spark job progress in the Spark Master UI
echo.
echo To view real-time logs:
echo   docker-compose logs -f [service-name]
echo   Examples:
echo     docker-compose logs -f hft-engine
echo     docker-compose logs -f pattern-consumer
echo     docker-compose logs -f market-producer
echo.
echo To stop the system:
echo   docker-compose down
echo.
echo To stop and remove all data:
echo   docker-compose down -v
echo.

REM Option to watch logs
set /p watch_logs="Would you like to watch real-time logs? (y/N): "
if /i "!watch_logs!"=="y" (
    echo.
    echo [INFO] Starting log aggregation... (Press Ctrl+C to exit)
    echo.
    docker-compose logs -f
)

echo.
echo [SUCCESS] Fractal Analysis Trading System is now running!
echo [INFO] Access the main dashboard at: http://localhost/
echo.
pause

endlocal