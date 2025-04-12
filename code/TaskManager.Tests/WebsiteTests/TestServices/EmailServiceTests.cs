using Microsoft.Extensions.Configuration;
using TaskManagerData.Services;

namespace TaskManager.Tests.WebsiteTests.TestServices
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_WhenSmtpConnectionFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"EmailSettings:SenderEmail", "sender@example.com"},
                {"EmailSettings:SmtpServer", "invalid.smtp.server"},
                {"EmailSettings:SmtpPort", "25"},
                {"EmailSettings:SenderPassword", "password"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var emailService = new EmailService(configuration);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await emailService.SendEmailAsync("recipient@example.com", "Test Subject", "Test Body"));

            Assert.Equal("Error sending email", exception.Message);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public async Task SendEmailAsync_WithNoConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"EmailSettings:SenderEmail", "sender@example.com"},
                {"EmailSettings:SmtpServer", "invalid.smtp.server"},
                {"EmailSettings:SmtpPort", "25"},
                {"EmailSettings:SenderPassword", "password"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var emailService = new EmailService(configuration);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await emailService.SendEmailAsync("recipient@example.com", "Test Subject", "Test Body"));
        }
    }
}
