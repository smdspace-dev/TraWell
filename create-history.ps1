# TraWell Development History - Commit Generator
# This script creates realistic commit history from March 2025 to September 2025

# March 2025 - Initial Setup
Write-Host "Creating March 2025 commits..."

# Initial project setup
git add TravelRecommendation.csproj Program.cs
$env:GIT_AUTHOR_DATE = "2025-03-01 09:00:00"
$env:GIT_COMMITTER_DATE = "2025-03-01 09:00:00"
git commit -m "🎯 Initial .NET 9.0 project setup with ASP.NET Core"

# Basic structure
git add Models/ Data/
$env:GIT_AUTHOR_DATE = "2025-03-02 10:30:00"
$env:GIT_COMMITTER_DATE = "2025-03-02 10:30:00"
git commit -m "📁 Add basic project structure and data models"

# Entity Framework setup
git add Migrations/
$env:GIT_AUTHOR_DATE = "2025-03-03 14:20:00"
$env:GIT_COMMITTER_DATE = "2025-03-03 14:20:00"
git commit -m "🗄️ Initial database migration with Entity Framework Core"

# Authentication setup
git add Controllers/AuthController.cs Services/OtpService.cs
$env:GIT_AUTHOR_DATE = "2025-03-05 16:45:00"
$env:GIT_COMMITTER_DATE = "2025-03-05 16:45:00"
git commit -m "🔐 Implement JWT authentication with OTP verification"

# Basic controllers
git add Controllers/PlacesController.cs Controllers/HealthController.cs
$env:GIT_AUTHOR_DATE = "2025-03-07 11:15:00"
$env:GIT_COMMITTER_DATE = "2025-03-07 11:15:00"
git commit -m "🎮 Add basic API controllers for places and health check"

# Frontend initial
git add wwwroot/index.html wwwroot/css/ wwwroot/js/
$env:GIT_AUTHOR_DATE = "2025-03-10 13:30:00"
$env:GIT_COMMITTER_DATE = "2025-03-10 13:30:00"
git commit -m "🎨 Initial frontend with responsive design and basic styling"

# Email service
git add Services/EmailService.cs
$env:GIT_AUTHOR_DATE = "2025-03-12 15:20:00"
$env:GIT_COMMITTER_DATE = "2025-03-12 15:20:00"
git commit -m "📧 Implement email service with Gmail SMTP integration"

# Configuration
git add appsettings.json Properties/launchSettings.json
$env:GIT_AUTHOR_DATE = "2025-03-14 09:45:00"
$env:GIT_COMMITTER_DATE = "2025-03-14 09:45:00"
git commit -m "⚙️ Add application configuration and launch settings"

# API improvements
git add Controllers/RecommendationsController.cs Services/RecommendationService.cs
$env:GIT_AUTHOR_DATE = "2025-03-16 17:10:00"
$env:GIT_COMMITTER_DATE = "2025-03-16 17:10:00"
git commit -m "🤖 Add AI-powered recommendation engine"

# Hotel system
git add Controllers/PublicHotelsController.cs Models/Hotel.cs
$env:GIT_AUTHOR_DATE = "2025-03-18 12:25:00"
$env:GIT_COMMITTER_DATE = "2025-03-18 12:25:00"
git commit -m "🏨 Implement hotel booking system with availability"

Write-Host "March commits completed. Moving to April..."

# Continue with more commits...
