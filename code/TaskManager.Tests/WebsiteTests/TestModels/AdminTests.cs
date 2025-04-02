using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class AdminTests
    {
        [Fact]
        public void Admin_Id_CanBeSetAndRetrieved()
        {
            var admin = new Admin { Id = 1 };
            Assert.Equal(1, admin.Id);
        }

        [Fact]
        public void Admin_UserId_CanBeSetAndRetrieved()
        {
            var admin = new Admin { UserId = 123 };
            Assert.Equal(123, admin.UserId);
        }

        [Fact]
        public void Admin_User_CanBeAssignedAndRetrieved()
        {
            var user = new User { Id = 1, UserName = "TestUser" };
            var admin = new Admin { User = user };

            Assert.NotNull(admin.User);
            Assert.Equal(1, admin.User.Id);
            Assert.Equal("TestUser", admin.User.UserName);
        }

        [Fact]
        public void Admin_DefaultConstructor_InitializesObject()
        {
            var admin = new Admin();
            Assert.NotNull(admin);
        }

        [Fact]
        public void Admin_CanBeInstantiated_WithValues()
        {
            var user = new User { Id = 2, UserName = "AdminUser" };
            var admin = new Admin { Id = 5, UserId = 2, User = user };

            Assert.Equal(5, admin.Id);
            Assert.Equal(2, admin.UserId);
            Assert.Equal(user, admin.User);
        }
    }
}