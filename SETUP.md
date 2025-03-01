# 🛠️ TraWell Development Setup Guide

## Quick Setup

### 1. Clone & Configure
```bash
git clone https://github.com/yourusername/trawell.git
cd trawell
cp appsettings.example.json appsettings.Development.json
```

### 2. Configure Email (Required for OTP)
Edit `appsettings.Development.json`:
```json
{
  "EmailSettings": {
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "FromEmail": "your-email@gmail.com"
  }
}
```

#### Gmail App Password Setup:
1. Enable 2FA: Google Account → Security → 2-Step Verification
2. Generate App Password: Security → App passwords → Select app → Generate
3. Use the 16-character password in config

### 3. Run
```bash
dotnet restore
dotnet run
```

## 📊 Environment Information

### Database Files
- **Development**: `TraWell-Dev.db` (auto-created, SQLite)
- **Production**: `TraWell.db` (SQLite)

Both files are automatically excluded from Git via `.gitignore`

### Sensitive Files (Git Ignored)
✅ **Database files**: `*.db`, `TraWell*.db`  
✅ **Configuration**: `appsettings.Development.json`, `appsettings.Production.json`  
✅ **Build output**: `bin/`, `obj/`  
✅ **IDE files**: `.vs/`, `.vscode/`, `.idea/`  
✅ **Environment files**: `.env*`

## 🔒 Security Checklist

### Before First Commit:
- [ ] Database files excluded (`.gitignore` configured)
- [ ] SMTP credentials in `appsettings.Development.json` (git ignored)
- [ ] JWT secret key changed from default
- [ ] Razorpay keys configured (for payments)

### For Production:
- [ ] Use strong JWT secret (256-bit minimum)
- [ ] Configure production database connection
- [ ] Set up proper SMTP service
- [ ] Configure rate limiting
- [ ] Enable HTTPS

## 🧪 Testing

```bash
# Test OTP flow
curl -X POST http://localhost:5088/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}'
```

## 📝 Development Notes

- **Database**: SQLite for development, easily switchable to PostgreSQL/SQL Server
- **OTP Flow**: Email verification required for user registration
- **Authentication**: JWT with refresh tokens
- **Payments**: Razorpay integration for bookings
