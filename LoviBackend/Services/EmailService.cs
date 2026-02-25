using System.Net;
using System.Net.Mail;

namespace LoviBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var section = _configuration.GetSection("Email");
            var host = section["Host"];
            var port = Convert.ToInt32(section["Port"]);
            var username = section["Username"];
            var password = section["Password"];
            var from = section["From"];
            var enableSsl = Convert.ToBoolean(section["EnableSsl"]);

            using var message = new MailMessage();
            message.From = new MailAddress(from!);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;

            var fullHtml = BuildEmailHtml(subject, htmlBody);
            message.Body = fullHtml;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrEmpty(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message);
        }

        private string BuildEmailHtml(string subject, string contentHtml)
        {
            var section = _configuration.GetSection("App");
            var baseUrl = section["BaseUrl"] ?? string.Empty;
            var logoUrl = section["LogoUrl"] ?? string.Empty;
            var backgroundColor = "#231f20";
            var textColor = "#ffffff";
            var fontFamily = "'Noto Serif',serif";
            var fontSize = "large";

            // logo
            var logoImg = string.Empty;
            if (!string.IsNullOrEmpty(logoUrl))
            {
                logoImg = $"<a href=\"{baseUrl}\" title=\"LOVI\"><img src=\"{logoUrl}\" alt=\"logo\" style=\"max-width:160px;height:auto;display:inline-block;\" /></a>";
            }

            // signature
            var signatureHtml = $"<strong>The LOVI Team</strong>";

            return $@"
                <!doctype html>
                <html>
                <head>
                    <meta charset=""utf-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>{subject}</title>
                </head>
                <body style=""margin:0;padding:0;background-color:{backgroundColor};color:{textColor};font-family:{fontFamily};font-size:{fontSize};text-align:center;"">
                    <div style=""padding:1rem 0;"">
                        <table role=""presentation"" width=""100%"" style=""border-collapse:collapse;"">
                        <tr>
                            <td style=""padding:1rem;"">
                                {logoImg}
                            </td>
                        </tr>
                        <tr>
                            <td style=""padding:1rem;"">
                                {contentHtml}
                            </td>
                        </tr>
                        <tr>
                            <td style=""padding:1rem;"">
                                {signatureHtml}
                            </td>
                        </tr>
                        </table>
                    </div>
                </body>
                </html>
            ";
        }
    }
}
