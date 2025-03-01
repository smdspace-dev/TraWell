#!/bin/bash

# TraWell Production Deployment Script
# This script automates the deployment process for the TraWell application

set -e  # Exit on any error

# Configuration
APP_NAME="TraWell"
COMPOSE_FILE="docker-compose.yml"
BACKUP_DIR="./backups/$(date +%Y%m%d_%H%M%S)"
LOG_FILE="./logs/deployment.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
    log "INFO: $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
    log "SUCCESS: $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
    log "WARNING: $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
    log "ERROR: $1"
}

# Pre-deployment checks
pre_deployment_checks() {
    print_status "Running pre-deployment checks..."
    
    # Check if Docker is installed and running
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker is not running. Please start Docker service."
        exit 1
    fi
    
    # Check if docker-compose is available
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not available. Please install Docker Compose."
        exit 1
    fi
    
    # Check if required files exist
    if [ ! -f "$COMPOSE_FILE" ]; then
        print_error "Docker compose file not found: $COMPOSE_FILE"
        exit 1
    fi
    
    if [ ! -f "Dockerfile" ]; then
        print_error "Dockerfile not found"
        exit 1
    fi
    
    print_success "Pre-deployment checks completed"
}

# Create backup
create_backup() {
    print_status "Creating backup..."
    
    mkdir -p "$BACKUP_DIR"
    
    # Backup database if container exists
    if docker ps -a --format "table {{.Names}}" | grep -q "trawell-database"; then
        print_status "Backing up database..."
        docker exec trawell-database pg_dump -U trawell_user trawell_prod > "$BACKUP_DIR/database_backup.sql" 2>/dev/null || true
    fi
    
    # Backup application data
    if [ -d "./data" ]; then
        print_status "Backing up application data..."
        cp -r ./data "$BACKUP_DIR/"
    fi
    
    # Backup logs
    if [ -d "./logs" ]; then
        print_status "Backing up logs..."
        cp -r ./logs "$BACKUP_DIR/"
    fi
    
    print_success "Backup created at: $BACKUP_DIR"
}

# Stop existing services
stop_services() {
    print_status "Stopping existing services..."
    
    # Check if we're using docker-compose or docker compose
    if command -v docker-compose &> /dev/null; then
        docker-compose down || true
    else
        docker compose down || true
    fi
    
    print_success "Services stopped"
}

# Build and deploy
deploy() {
    print_status "Building and deploying $APP_NAME..."
    
    # Create necessary directories
    mkdir -p ./data ./logs ./nginx
    
    # Build and start services
    if command -v docker-compose &> /dev/null; then
        docker-compose up -d --build
    else
        docker compose up -d --build
    fi
    
    print_success "Deployment completed"
}

# Verify deployment
verify_deployment() {
    print_status "Verifying deployment..."
    
    # Wait for services to be healthy
    local max_attempts=30
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        print_status "Checking application health (attempt $((attempt + 1))/$max_attempts)..."
        
        if curl -f http://localhost:8080/api/health/live &> /dev/null; then
            print_success "Application is healthy and running"
            return 0
        fi
        
        sleep 10
        ((attempt++))
    done
    
    print_error "Application failed to start properly"
    return 1
}

# Show service status
show_status() {
    print_status "Service Status:"
    
    if command -v docker-compose &> /dev/null; then
        docker-compose ps
    else
        docker compose ps
    fi
    
    echo ""
    print_status "Application URLs:"
    echo "  Main Application: http://localhost:8080"
    echo "  Health Check: http://localhost:8080/api/health"
    echo "  Admin Panel: http://localhost:8080/admin.html"
}

# Rollback function
rollback() {
    print_warning "Rolling back to previous version..."
    
    # Stop current services
    stop_services
    
    # Restore from latest backup
    local latest_backup=$(ls -t backups/ | head -n1)
    if [ -n "$latest_backup" ]; then
        print_status "Restoring from backup: $latest_backup"
        
        # Restore data
        if [ -d "backups/$latest_backup/data" ]; then
            rm -rf ./data
            cp -r "backups/$latest_backup/data" ./
        fi
        
        # Restore database
        if [ -f "backups/$latest_backup/database_backup.sql" ]; then
            # Start only database service for restore
            if command -v docker-compose &> /dev/null; then
                docker-compose up -d trawell-db
            else
                docker compose up -d trawell-db
            fi
            
            sleep 10
            docker exec -i trawell-database psql -U trawell_user trawell_prod < "backups/$latest_backup/database_backup.sql"
        fi
        
        print_success "Rollback completed"
    else
        print_error "No backup found for rollback"
        exit 1
    fi
}

# Main deployment function
main() {
    echo "========================================"
    echo "   TraWell Production Deployment"
    echo "========================================"
    echo ""
    
    # Create logs directory
    mkdir -p logs
    
    case "${1:-deploy}" in
        "deploy")
            pre_deployment_checks
            create_backup
            stop_services
            deploy
            verify_deployment
            show_status
            print_success "Deployment completed successfully!"
            ;;
        "rollback")
            rollback
            ;;
        "status")
            show_status
            ;;
        "stop")
            stop_services
            ;;
        "logs")
            if command -v docker-compose &> /dev/null; then
                docker-compose logs -f
            else
                docker compose logs -f
            fi
            ;;
        *)
            echo "Usage: $0 {deploy|rollback|status|stop|logs}"
            echo ""
            echo "Commands:"
            echo "  deploy   - Deploy the application (default)"
            echo "  rollback - Rollback to previous version"
            echo "  status   - Show service status"
            echo "  stop     - Stop all services"
            echo "  logs     - View application logs"
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"
