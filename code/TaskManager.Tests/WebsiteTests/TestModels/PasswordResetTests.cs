using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerData.Models;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class PasswordResetTests
    {
        [Fact]
        public void PasswordReset_WithValidData_ShouldBeValid()
        {
            var model = new PasswordReset
            {
                Code = "123456",
                Email = "test@example.com",
                Username = "testuser"
            };

            var validationResults = ValidateModel(model);

            Assert.Empty(validationResults); 
        }

        [Fact]
        public void PasswordReset_WithInvalidEmail_ShouldFailValidation()
        {
            var model = new PasswordReset
            {
                Code = "123456",
                Email = "invalid-email",
                Username = "testuser"
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void PasswordReset_WithShortCode_ShouldFailValidation()
        {
            var model = new PasswordReset
            {
                Code = "123", // too short
                Email = "test@example.com",
                Username = "testuser"
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("Code"));
        }

        [Fact]
        public void PasswordReset_WithMissingUsername_ShouldFailValidation()
        {
            var model = new PasswordReset
            {
                Code = "123456",
                Email = "test@example.com",
                Username = null
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void PasswordReset_Id_ShouldBeSet()
        {
            var reset = new PasswordReset { Id = 42 };
            Assert.Equal(42, reset.Id);
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
            return validationResults;
        }
    }
}
