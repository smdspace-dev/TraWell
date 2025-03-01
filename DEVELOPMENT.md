# TraWell - Development Guide (Private)

This is the private development documentation for TraWell project.

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Git
- Gmail account for SMTP testing
- Razorpay test account

### Environment Configuration

#### Gmail SMTP Setup
1. Enable 2-factor authentication on your Gmail account
2. Generate an App Password: Google Account → Security → App passwords
3. Use these credentials in `appsettings.Development.json`:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "ahilxdesigns@gmail.com",
       "SenderPassword": "hsod them udod wftf"
     }
   }
   ```

#### Razorpay Setup
Demo credentials for testing:
```json
{
  "RazorpaySettings": {
    "KeyId": "rzp_test_demo123456789",
    "KeySecret": "demo_secret_key_123456789"
  }
}
```

### Database Details
- **Location**: `C:\Users\thous\OneDrive\Desktop\.Net\TravelRecommendation\TraWell.db`
- **Type**: SQLite for development
- **Auto-migration**: Yes, runs on startup
- **Seeding**: Automatic sample data creation

### Authentication Flow Implementation

#### OTP Registration Process
1. User submits registration form → `POST /api/auth/register`
2. Server generates 6-digit OTP and stores in memory cache
3. OTP sent via Gmail SMTP to user's email
4. User enters OTP → `POST /api/auth/verify-otp`
5. Server validates OTP and creates user account
6. Returns JWT token for immediate login

#### Key Files Modified
- `Controllers/AuthController.cs` - Registration endpoint now sends OTP
- `Services/OtpService.cs` - Handles OTP generation and validation
- `wwwroot/js/app.js` - Frontend OTP modal handling
- `wwwroot/index.html` - OTP verification modal

### Fixed Issues Log

#### Issue 1: Direct Login Without OTP (Fixed)
**Problem**: Registration was creating users immediately without OTP verification
**Solution**: Modified registration endpoint to generate OTP and defer user creation until verification
**Files Changed**: `Controllers/AuthController.cs`
**Status**: ✅ Fixed

#### Issue 2: Compilation Errors (Fixed)
**Problem**: Duplicate `IOtpService` interface causing accessibility issues
**Solution**: Removed duplicate internal interface declaration
**Files Changed**: `Controllers/AuthController.cs`
**Status**: ✅ Fixed

#### Issue 3: API Endpoints Returning HTML (Fixed)
**Problem**: Non-existent `/api/public/` endpoints returning 404 HTML pages
**Solution**: Replaced API calls with sample data functions
**Files Changed**: `wwwroot/js/app.js`
**Status**: ✅ Fixed

#### Issue 4: Admin Authentication Loops (Fixed)
**Problem**: `admin.js` loading on all pages causing continuous auth checks
**Solution**: Removed admin.js from auth.html, only loads on admin pages
**Files Changed**: `wwwroot/auth.html`
**Status**: ✅ Fixed

### Code Architecture

#### Controllers
- `AuthController` - Authentication endpoints (register, login, verify-otp)
- `AdminController` - Admin dashboard functionality
- `BookingController` - Hotel/package booking logic

#### Services
- `EmailService` - Gmail SMTP integration for welcome/OTP emails
- `OtpService` - OTP generation, storage, and validation
- `RazorpayService` - Payment processing integration
- `DataSeeder` - Database sample data creation

#### Frontend Structure
- `index.html` - Main landing page with booking functionality
- `auth.html` - Login/registration page with OTP verification
- `admin.html` - Admin dashboard (admin users only)
- `js/app.js` - Main application logic and API calls
- `js/admin.js` - Admin panel functionality (separate from main app)

### Testing Accounts

#### Default Admin
- Email: `admin@trawell.com`
- Password: `Admin@123`
- Roles: Admin

#### Test User (Create via Registration)
- Any email address
- Must complete OTP verification
- Role: User (auto-assigned)

### Development Workflow

1. **Start Development Server**
   ```bash
   cd TravelRecommendation
   dotnet run
   ```

2. **Test Registration Flow**
   - Navigate to `/auth.html`
   - Register with real email address
   - Check email for OTP (6-digit code)
   - Complete verification
   - Should receive welcome email

3. **Test Admin Functions**
   - Login with admin account
   - Should auto-redirect to `/admin.html`
   - Access admin dashboard features

### Debugging Tips

#### Check Logs for OTP Issues
- Look for "OTP generated for: {email}" in console
- Check "OTP sent successfully" or email sending errors
- Verify SMTP credentials if email not received

#### Database Issues
- Delete `TraWell.db` to reset database
- Restart application to recreate with fresh data
- Check Entity Framework logs in console

#### Frontend Debugging
- Use browser Developer Tools Console
- Check Network tab for API call responses
- Verify JWT tokens in Local Storage

### Security Considerations

#### JWT Token Management
- Tokens stored in `localStorage` as `authToken`
- Automatic expiration handling
- Role-based route protection

#### OTP Security
- 6-digit random generation
- 10-minute expiration
- Memory cache storage (development only)
- One-time use validation

#### Email Security
- App passwords instead of main password
- SMTP over TLS (port 587)
- No sensitive data in email content

### Deployment Notes

#### Environment Differences
- **Development**: SQLite database, Gmail SMTP, console logging
- **Production**: PostgreSQL database, production email service, file logging

#### Configuration Management
- Use environment variables for sensitive settings
- Separate `appsettings.Production.json` for prod values
- Never commit credentials to version control

### Known Limitations

1. OTP storage in memory cache (loses on restart)
2. Sample data only (no real hotel/package data)
3. Basic email templates (no HTML formatting)
4. Single-instance deployment (no clustering support)

### Future Enhancements

1. Redis for OTP storage in production
2. Real hotel/package data integration
3. Advanced email templates with branding
4. Payment webhook handling
5. User profile management
6. Booking history and cancellation

### Contact Information

**Developer**: Development Team
**Email**: dev@trawell.com (internal only)
**Last Updated**: 2025-09-21
