using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class FindUsernameViewModelTests
    {
        /// <summary>
        /// Valid model should pass validation.
        /// </summary>
        [Fact]
        public void FindUsernameViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new FindUsernameViewModel
            {
                Email = "user@example.com"
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
        /// Model should fail validation when email is missing.
        /// </summary>
        [Fact]
        public void FindUsernameViewModel_MissingEmail_FailsValidation()
        {
            // Arrange
            var model = new FindUsernameViewModel
            {
                Email = ""
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            // Act
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Email is required.");
        }
    }
}
