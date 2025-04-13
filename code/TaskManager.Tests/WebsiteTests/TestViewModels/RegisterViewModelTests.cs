using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.WebsiteTests.TestViewModels
{
    public class RegisterViewModelTests
    {
        [Fact]
        public void TestRegisterViewModel()
        {
            var registerViewModel = new RegisterViewModel
            {
                UserName = "testUser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            Assert.Equal("testUser", registerViewModel.UserName);
            Assert.Equal("test@example.com", registerViewModel.Email);
            Assert.Equal("password123", registerViewModel.Password);
            Assert.Equal("password123", registerViewModel.ConfirmPassword);

        }
    }
}
