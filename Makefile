# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help clean restore build test run docker-build docker-up docker-down publish format lint migrate

# Default target
help:
	@echo "dotnet-feature-flags build targets:"
	@echo ""
	@echo "  make restore      - Restore NuGet packages"
	@echo "  make clean        - Clean build artifacts"
	@echo "  make build        - Build the solution (Debug)"
	@echo "  make build-rel    - Build the solution (Release)"
	@echo "  make test         - Run tests"
	@echo "  make test-coverage - Run tests with coverage"
	@echo "  make run          - Run application locally"
	@echo "  make publish      - Publish for production"
	@echo "  make format       - Format code with dotnet format"
	@echo "  make lint         - Run code analysis"
	@echo "  make docker-build - Build Docker image"
	@echo "  make docker-up    - Start Docker containers (docker-compose)"
	@echo "  make docker-down  - Stop Docker containers"
	@echo "  make docker-logs  - View Docker logs"
	@echo "  make migrate      - Run database migrations"
	@echo "  make migrate-add  - Add new migration (usage: make migrate-add NAME=MigrationName)"
	@echo "  make migrate-revert - Revert last migration"
	@echo "  make db-drop      - Drop database (DANGER!)"
	@echo ""

# Restore NuGet packages
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf ./bin ./obj
	rm -rf ./src/*/bin ./src/*/obj
	rm -rf ./publish

# Build Debug
build: restore
	@echo "Building (Debug)..."
	dotnet build

# Build Release
build-rel: restore
	@echo "Building (Release)..."
	dotnet build -c Release

# Run tests
test: build
	@echo "Running tests..."
	dotnet test --no-build --logger "console;verbosity=normal"

# Run tests with coverage
test-coverage: build
	@echo "Running tests with coverage..."
	dotnet test --no-build /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run application locally
run: build
	@echo "Starting application..."
	dotnet run --project src/FeatureFlags/FeatureFlags.csproj

# Run in Release mode
run-rel: build-rel
	@echo "Starting application (Release)..."
	dotnet run --project src/FeatureFlags/FeatureFlags.csproj --configuration Release

# Publish for production
publish: clean build-rel
	@echo "Publishing for production..."
	dotnet publish src/FeatureFlags/FeatureFlags.csproj -c Release -o ./publish
	@echo "Published to: ./publish"

# Format code
format:
	@echo "Formatting code..."
	dotnet format

# Run code analysis
lint:
	@echo "Running code analysis..."
	dotnet build /p:EnforceCodeStyleInBuild=true

# Docker: Build image
docker-build:
	@echo "Building Docker image..."
	docker build -t feature-flags:latest .

# Docker: Start containers
docker-up: docker-build
	@echo "Starting Docker containers..."
	docker-compose up -d
	@echo "API: http://localhost:5000"
	@echo "Database: localhost:1433"

# Docker: Stop containers
docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

# Docker: View logs
docker-logs:
	@echo "Docker logs (api):"
	docker-compose logs -f api

# Database: Run migrations
migrate:
	@echo "Running database migrations..."
	dotnet ef database update --project src/FeatureFlags

# Database: Add new migration
migrate-add:
	@if [ -z "$(NAME)" ]; then \
		echo "Error: NAME is required. Usage: make migrate-add NAME=MigrationName"; \
		exit 1; \
	fi
	@echo "Adding migration: $(NAME)"
	dotnet ef migrations add $(NAME) --project src/FeatureFlags

# Database: Revert last migration
migrate-revert:
	@echo "Reverting last migration..."
	dotnet ef database update 0 --project src/FeatureFlags

# Database: Drop database (DANGER!)
db-drop:
	@echo "WARNING: This will DELETE the database!"
	@read -p "Type 'YES' to confirm: " confirm; \
	if [ "$$confirm" = "YES" ]; then \
		dotnet ef database drop -f --project src/FeatureFlags; \
		echo "Database dropped"; \
	else \
		echo "Cancelled"; \
	fi

# Additional useful targets

# Install dependencies for development
setup: restore
	@echo "Setup complete. Run 'make run' to start the application."

# Run everything needed for a clean start
init: clean restore migrate run

# Pre-commit checks
pre-commit: format lint test
	@echo "Pre-commit checks passed!"

# Build for CI/CD
ci: clean restore lint test
	@echo "CI pipeline complete!"

# Create release build
release: clean build-rel test publish
	@echo "Release build complete! Check ./publish directory."

# Watch for changes and rebuild
watch:
	@echo "Watching for changes..."
	dotnet watch --project src/FeatureFlags/FeatureFlags.csproj run

# Generate API documentation
docs:
	@echo "Generating API documentation..."
	dotnet build -c Release /p:GenerateDocumentationFile=true
	@echo "Check bin/Release/net10.0/FeatureFlags.xml for generated docs"

.SILENT: help
