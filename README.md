# 🌍 TraWell - Travel Recommendation Platform

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity_Framework-512BD4?style=for-the-badge&logo=microsoft&logoColor=white)](https://docs.microsoft.com/en-us/ef/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://sqlite.org/)

[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=for-the-badge)](CONTRIBUTING.md)
[![Build Status](https://img.shields.io/badge/Build-Passing-success?style=for-the-badge)](https://github.com/yourusername/trawell)

</div>

## 📖 Overview

TraWell is a comprehensive travel recommendation platform built with modern .NET technologies. It provides personalized travel suggestions, hotel bookings, tour packages, and seamless travel planning experiences with advanced authentication and payment integration.

## ✨ Key Features

### 🔐 Authentication & Security
- **JWT-based Authentication** with role-based access control
- **OTP Email Verification** for secure user registration
- **Multi-role Support** (Admin, User, Provider)
- **Password Security** with ASP.NET Core Identity

### 🎯 Travel Services
- **AI-Powered Recommendations** based on user preferences
- **Hotel Booking System** with real-time availability
- **Tour Package Management** with customizable itineraries
- **Location-based Services** with comprehensive place database

### 💳 Payment & Booking
- **Razorpay Integration** for secure payment processing
- **Booking Management** with confirmation emails
- **Invoice Generation** and transaction history

### 📱 User Experience
- **Responsive Web Design** with modern UI/UX
- **Real-time Notifications** via email integration
- **Profile Management** with personalized dashboards
- **Search & Filter** capabilities

## 🏗️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Language**: C# 12.0
- **ORM**: Entity Framework Core
- **Database**: SQLite (Development), PostgreSQL (Production Ready)
- **Authentication**: ASP.NET Core Identity + JWT
- **Email**: Gmail SMTP Integration

### Frontend
- **HTML5** with modern CSS3
- **JavaScript ES6+** for interactive components
- **Bootstrap 5** for responsive design
- **Real-time UI** updates

### DevOps & Tools
- **Development**: Visual Studio 2024 / VS Code
- **Package Manager**: NuGet
- **Version Control**: Git
- **API Documentation**: OpenAPI/Swagger

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- Email account for SMTP (Gmail recommended)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/trawell.git
   cd trawell
   ```

2. **Configure application settings**
   ```bash
   # Copy example configuration
   cp appsettings.example.json appsettings.Development.json
   ```

3. **Update configuration** (see [Configuration Guide](#-configuration))
   - Email settings for OTP verification
   - JWT secret key
   - Payment gateway credentials

4. **Install dependencies & run**
   ```bash
   dotnet restore
   dotnet run
   ```

5. **Access the application**
   - Open: `http://localhost:5088`
   - API Documentation: `http://localhost:5088/swagger`

## ⚙️ Configuration

### Required Settings

Create `appsettings.Development.json` with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TraWell-Dev.db"
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key",
    "Issuer": "TraWell-Dev",
    "Audience": "TraWell-Dev-Users"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "TraWell Support"
  }
}
```

### Email Configuration
1. Enable 2-factor authentication on Gmail
2. Generate App Password: Google Account → Security → App passwords
3. Use the generated password in `EmailSettings.Password`

## 📚 API Documentation

### Authentication Endpoints
- `POST /auth/register` - User registration with OTP
- `POST /auth/verify-otp` - OTP verification
- `POST /auth/login` - User login
- `POST /auth/refresh` - Token refresh

### Core Features
- `GET /places` - Get travel destinations
- `GET /hotels` - Hotel search and listings
- `GET /packages` - Tour package catalog
- `POST /bookings` - Create booking
- `GET /recommendations` - Personalized suggestions

## 🗄️ Database Information

### Database Files
- **Development**: `TraWell-Dev.db` (SQLite)
- **Production**: `TraWell.db` (SQLite)

### Key Entities
- Users & Roles (ASP.NET Identity)
- Places & Destinations
- Hotels & Accommodations
- Tour Packages & Bookings
- Reviews & Ratings

## 🛡️ Security Features

- **Password Hashing** with ASP.NET Core Identity
- **JWT Token Security** with refresh token support
- **OTP Verification** for email confirmation
- **Rate Limiting** for API endpoints
- **Input Validation** and sanitization
- **CORS Configuration** for secure cross-origin requests

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test TraWell.Tests/
dotnet test TraWell.IntegrationTests/
```

## 📦 Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
```bash
dotnet publish -c Release -o ./publish
dotnet ./publish/TraWell.dll
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Workflow
1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors & Acknowledgments

- **Development Team** - Initial work and ongoing maintenance
- **Contributors** - Thanks to all contributors who have helped improve this project

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/trawell/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/trawell/discussions)
- **Email**: support@trawell.com

## 🔗 Links

- [Live Demo](https://trawell-demo.azurewebsites.net)
- [API Documentation](https://trawell-api-docs.com)
- [Project Roadmap](https://github.com/yourusername/trawell/projects)

---

<div align="center">
  Made with ❤️ by the TraWell Team
</div>
