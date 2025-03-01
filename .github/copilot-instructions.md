<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Travel Recommendation Application - Copilot Instructions

This is a .NET Web API project for a travel recommendation system that suggests places to visit in India based on user preferences.

## Project Context

- **Technology Stack**: ASP.NET Core Web API, MySQL, ML.NET, YoutubeExplode, HTML/CSS/JavaScript frontend
- **Purpose**: Travel recommendation system with YouTube video integration and AI-powered suggestions
- **Current Status**: Core infrastructure complete, MySQL connection needs fixing

## Code Style Guidelines

- Use **C# 12** features and modern .NET patterns
- Follow **async/await** patterns for all database and API calls
- Use **dependency injection** for all services
- Implement **proper error handling** with try-catch blocks
- Use **record types** for DTOs where appropriate
- Follow **RESTful API** conventions

## Key Components

1. **Models**: Place, UserPreferences, Video entities
2. **Services**: PlaceService (database), VideoService (YouTube), RecommendationService (ML.NET)
3. **Controllers**: RecommendationsController, PlacesController
4. **Frontend**: Vanilla JavaScript with Leaflet.js for mapping

## Current Priorities

1. Fix MySQL connection issues in PlaceService
2. Enhance recommendation algorithm with ML.NET
3. Improve YouTube video scraping reliability
4. Add more Indian travel destinations to database
5. Enhance frontend user experience

## Development Notes

- **Database**: Currently using in-memory data, needs MySQL integration
- **API Endpoints**: All endpoints should return proper HTTP status codes
- **Error Handling**: Implement global exception handling middleware
- **Logging**: Use ILogger for all services
- **Testing**: Add unit tests for services and controllers

## When generating code:

- Always include proper error handling
- Use modern C# syntax and patterns
- Include XML documentation for public methods
- Follow the existing project structure
- Consider performance implications for video scraping
- Implement proper validation for user inputs
