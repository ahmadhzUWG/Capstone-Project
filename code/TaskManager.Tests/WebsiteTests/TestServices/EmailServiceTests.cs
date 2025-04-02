using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Xunit;
using TaskManagerWebsite.Services;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace TaskManagerWebsite.Tests
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
