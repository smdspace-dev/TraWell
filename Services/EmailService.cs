using System.Net;
using System.Net.Mail;

namespace TraWell.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp, string userName);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp, string userName)
        {
            try
            {
                var subject = "TraWell - Email Verification Code";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 30px; background: #f9f9f9; }}
                            .otp-code {{ background: #4CAF50; color: white; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0; border-radius: 8px; }}
                            .footer {{ padding: 20px; text-align: center; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🌍 TraWell</h1>
                                <p>Your Travel Companion</p>
                            </div>
                            <div class='content'>
                                <h2>Hello {userName}!</h2>
                                <p>Thank you for registering with TraWell. Please use the following verification code to complete your registration:</p>
                                <div class='otp-code'>{otp}</div>
                                <p>This code will expire in 10 minutes for security purposes.</p>
                                <p>If you didn't request this verification, please ignore this email.</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; 2025 TraWell. All rights reserved.</p>
                                <p>Happy Travels! ✈️</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "Welcome to TraWell! 🌍";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 30px; background: #f9f9f9; }}
                            .feature {{ background: white; padding: 15px; margin: 10px 0; border-radius: 8px; border-left: 4px solid #4CAF50; }}
                            .footer {{ padding: 20px; text-align: center; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🌍 Welcome to TraWell!</h1>
                                <p>Your Journey Begins Here</p>
                            </div>
                            <div class='content'>
                                <h2>Hello {userName}!</h2>
                                <p>Congratulations! Your TraWell account has been successfully verified and activated.</p>
                                
                                <h3>What you can do now:</h3>
                                <div class='feature'>
                                    <strong>🏨 Book Hotels</strong> - Find and book amazing hotels worldwide
                                </div>
                                <div class='feature'>
                                    <strong>✈️ Tour Packages</strong> - Discover curated travel experiences
                                </div>
                                <div class='feature'>
                                    <strong>🗺️ Explore Destinations</strong> - Get personalized recommendations
                                </div>
                                <div class='feature'>
                                    <strong>📱 Manage Bookings</strong> - Track and manage all your reservations
                                </div>
                                
                                <p>Start exploring amazing destinations and create unforgettable memories!</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; 2025 TraWell. All rights reserved.</p>
                                <p>Safe travels! 🧳✨</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:Username"] ?? "demo@trawell.com";
            var smtpPassword = _configuration["EmailSettings:Password"] ?? "demo-password";
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@trawell.com";

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, "TraWell");
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                // Actually send the email now
                await client.SendMailAsync(message);
                _logger.LogInformation("✅ Email sent successfully to: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
