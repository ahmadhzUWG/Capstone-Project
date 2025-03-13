using MimeKit;
using MailKit.Net.Smtp;

namespace TaskManagerWebsite.Services
{
    /// <summary>
    /// Represents an email service.
    /// </summary>
    public class EmailService
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        public EmailService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <exception cref="System.InvalidOperationException">Error sending email</exception>
        public virtual async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Task Manager", emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), false);
                    
                    await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);

                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error sending email", ex);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
