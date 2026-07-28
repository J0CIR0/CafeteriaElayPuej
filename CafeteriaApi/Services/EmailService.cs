using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace CafeteriaApi.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }

    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string toEmail, string fullName, string code)
        {
            try
            {
                var subject = "Código de Verificación - Cafetería Elay Puej";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; }}
                            .header {{ background-color: #6f4e37; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .code {{ font-size: 36px; font-weight: bold; color: #6f4e37; text-align: center; padding: 20px; letter-spacing: 10px; }}
                            .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>☕ Cafetería Elay Puej</h2>
                            </div>
                            <div style='padding: 20px;'>
                                <h3>¡Hola {fullName}!</h3>
                                <p>Gracias por registrarte en Cafetería Elay Puej.</p>
                                <p>Tu código de verificación es:</p>
                                <div class='code'>{code}</div>
                                <p>Este código expirará en 10 minutos.</p>
                                <p>Si no solicitaste este código, ignora este mensaje.</p>
                            </div>
                            <div class='footer'>
                                <p>Cafetería Elay Puej - Café que cuenta la historia del origen</p>
                                <p>Santa Cruz de la Sierra, Bolivia</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código de verificación a {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string fullName, string code)
        {
            try
            {
                var subject = "Recuperación de Contraseña - Cafetería Elay Puej";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; }}
                            .header {{ background-color: #6f4e37; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .code {{ font-size: 36px; font-weight: bold; color: #6f4e37; text-align: center; padding: 20px; letter-spacing: 10px; }}
                            .footer {{ text-align: center; color: #888; font-size: 12px; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>☕ Cafetería Elay Puej</h2>
                            </div>
                            <div style='padding: 20px;'>
                                <h3>¡Hola {fullName}!</h3>
                                <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                                <p>Tu código de recuperación es:</p>
                                <div class='code'>{code}</div>
                                <p>Este código expirará en 10 minutos.</p>
                                <p>Si no solicitaste este código, ignora este mensaje.</p>
                            </div>
                            <div class='footer'>
                                <p>Cafetería Elay Puej - Café que cuenta la historia del origen</p>
                                <p>Santa Cruz de la Sierra, Bolivia</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código de recuperación a {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Correo enviado exitosamente a {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Email}", toEmail);
                return false;
            }
        }
    }
}