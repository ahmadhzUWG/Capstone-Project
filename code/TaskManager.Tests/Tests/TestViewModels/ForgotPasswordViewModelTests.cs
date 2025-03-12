using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class ForgotPasswordViewModelTests
    {
        /// <summary>
        /// Valid model should pass validation.
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Username = "adminUser",
                Password = "StrongPass1!",
                ConfirmPassword = "StrongPass1!"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        /// <summary>
        /// Model should fail validation when username is empty.
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_MissingUsername_FailsValidation()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Username = "",
                Password = "StrongPass1!",
                ConfirmPassword = "StrongPass1!"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Username is required");
        }

        /// <summary>
        /// Model should fail validation when password is missing.
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_MissingPassword_FailsValidation()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Username = "adminUser",
                Password = "",
                ConfirmPassword = "StrongPass1!"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage == "A password is required. Please use at least 8 characters, mixing letters and digits.");
        }

        /// <summary>
        /// Model should fail validation when confirm password is missing.
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_MissingConfirmPassword_FailsValidation()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Username = "adminUser",
                Password = "StrongPass1!",
                ConfirmPassword = ""
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Please confirm your password to ensure accuracy.");
        }

        /// <summary>
        /// Model should fail validation when passwords do not match.
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_PasswordMismatch_FailsValidation()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                Username = "adminUser",
                Password = "StrongPass1!",
                ConfirmPassword = "WrongPass2!"
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Passwords do not match. Make sure both fields are the same.");
        }
    }
}
