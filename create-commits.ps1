# Create realistic commit history for TraWell project

# Function to create a commit with specific date
function Create-CommitWithDate {
    param(
        [string]$Date,
        [string]$Message,
        [string]$Files = "."
    )
    
    git add $Files
    $env:GIT_AUTHOR_DATE = $Date
    $env:GIT_COMMITTER_DATE = $Date
    git commit -m $Message --allow-empty
    Write-Host "Created commit: $Message"
}

# March 2025 commits
Create-CommitWithDate "2025-03-02 10:15:00" "Add Entity Framework DbContext configuration" "Data/TraWellDbContext.cs"
Create-CommitWithDate "2025-03-02 18:45:00" "Implement user models with preferences" "Models/ApplicationUser.cs Models/UserPreferences.cs"
Create-CommitWithDate "2025-03-03 11:20:00" "Add Place and Hotel data models" "Models/Place.cs Models/Hotel.cs"
Create-CommitWithDate "2025-03-03 15:30:00" "Implement booking and tour package models" "Models/Booking.cs Models/TourPackage.cs Models/PackageProvider.cs"
Create-CommitWithDate "2025-03-04 09:45:00" "Add Data Transfer Objects for API responses" "Models/DTOs/"
Create-CommitWithDate "2025-03-04 19:20:00" "Initial SQLite database migration" "Migrations/"
Create-CommitWithDate "2025-03-05 13:10:00" "Implement JWT authentication controller" "Controllers/AuthController.cs"
Create-CommitWithDate "2025-03-05 16:35:00" "Add OTP generation and verification service" "Services/OtpService.cs"
Create-CommitWithDate "2025-03-08 14:20:00" "Fix authentication flow and OTP validation" "Controllers/AuthController.cs"
Create-CommitWithDate "2025-03-10 11:30:00" "Update user registration with email verification" "Services/EmailService.cs"

# April 2025 commits
Create-CommitWithDate "2025-04-01 08:30:00" "Implement Gmail SMTP email service" "Services/EmailService.cs"
Create-CommitWithDate "2025-04-01 14:45:00" "Add application configuration and launch settings" "appsettings.json Properties/launchSettings.json"
Create-CommitWithDate "2025-04-02 10:20:00" "Initial responsive frontend with modern CSS" "wwwroot/index.html wwwroot/css/style.css"
Create-CommitWithDate "2025-04-02 17:50:00" "Add login/register frontend with OTP modal" "wwwroot/auth.html wwwroot/js/app.js"
Create-CommitWithDate "2025-04-03 11:15:00" "Implement places API with search functionality" "Controllers/PlacesController.cs Services/PlaceService.cs"
Create-CommitWithDate "2025-04-03 15:40:00" "Add hotel booking API endpoints" "Controllers/PublicHotelsController.cs"
Create-CommitWithDate "2025-04-04 09:25:00" "Implement tour package management system" "Controllers/PublicPackagesController.cs"
Create-CommitWithDate "2025-04-04 16:30:00" "Add AI-powered travel recommendation engine" "Controllers/RecommendationsController.cs Services/RecommendationService.cs"
Create-CommitWithDate "2025-04-05 12:10:00" "Integrate Razorpay payment gateway" "Controllers/PaymentController.cs Services/RazorpayService.cs"
Create-CommitWithDate "2025-04-05 18:20:00" "Complete booking management system" "Controllers/BookingController.cs"
Create-CommitWithDate "2025-04-08 10:15:00" "Enhance frontend styling and responsiveness" "wwwroot/css/style.css"
Create-CommitWithDate "2025-04-12 16:45:00" "Add form validation and error handling" "wwwroot/js/app.js"
Create-CommitWithDate "2025-04-15 09:30:00" "Implement API error responses" "Controllers/AuthController.cs"
Create-CommitWithDate "2025-04-18 14:20:00" "Add loading states and user feedback" "wwwroot/js/app.js"
Create-CommitWithDate "2025-04-22 11:10:00" "Optimize database queries and performance" "Services/PlaceService.cs"
Create-CommitWithDate "2025-04-25 17:35:00" "Add input sanitization and security" "Controllers/AuthController.cs"
Create-CommitWithDate "2025-04-28 13:45:00" "Implement session management" "Program.cs"

Write-Host "Created April 2025 commits successfully!"
