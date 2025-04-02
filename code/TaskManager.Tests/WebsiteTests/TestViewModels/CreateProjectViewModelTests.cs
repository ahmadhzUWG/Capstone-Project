using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.ViewModels.ProjectViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class CreateProjectViewModelTests
    {
        [Fact]
        public void Name_ShouldBeRequired()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Name = null // Set Name to null to trigger the Required validation
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);
        }

        [Fact]
        public void Name_ShouldHaveValidLength()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Name = "A" // Name too short to trigger length validation
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);

            // Test for too long name
            model.Name = new string('A', 101); // Name too long
            validationResults.Clear();
            isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);
        }

        [Fact]
        public void Description_ShouldBeRequired()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Description = null // Set Description to null to trigger the Required validation
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);
        }

        [Fact]
        public void Description_ShouldNotExceedMaxLength()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Description = new string('A', 501) // Description exceeds 500 characters
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);
        }

        [Fact]
        public void ProjectLeadId_ShouldBeRequired()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                ProjectLeadId = 0 // Invalid selection for project lead
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Equal(2, validationResults.Count);
        }

    }
}
