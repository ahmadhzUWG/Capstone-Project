using System.ComponentModel.DataAnnotations;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class UserViewModelTests
    {
        [Fact]
        public void UserViewModel_Properties_CanBeSetAndRetrieved()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = "user@gmail.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            Assert.Equal("testUser", vm.UserName);
            Assert.Equal("user@gmail.com", vm.Email);
            Assert.Equal("password123", vm.Password);
            Assert.Equal("password123", vm.ConfirmPassword);
        }

        [Fact]
        public void UserViewModel_Validation_PassesForValidModel()
        {
            var vm = new UserViewModel
            {
                UserName = "validUser",
                Email = "user@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var results = ValidateModel(vm);
            Assert.Empty(results);
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenUserNameMissing()
        {
            var vm = new UserViewModel
            {
                UserName = null,
                Email = "user@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("UserName"));
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenEmailMissing()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = null,
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenEmailIsInvalid()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = "notAnEmail",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenPasswordMissing()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = "user@example.com",
                Password = null,
                ConfirmPassword = "password123"
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenConfirmPasswordMissing()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = "user@example.com",
                Password = "password123",
                ConfirmPassword = null
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void UserViewModel_Validation_Fails_WhenPasswordsDoNotMatch()
        {
            var vm = new UserViewModel
            {
                UserName = "testUser",
                Email = "user@example.com",
                Password = "password123",
                ConfirmPassword = "differentPassword"
            };

            var results = ValidateModel(vm);
            Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }
    }
}
