using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class UserTests
    {
        [Fact]
        public void User_Properties_CanBeSetAndRetrieved()
        {
            var user = new User
            {
                UserName = "testUser",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };

            Assert.Equal("testUser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("1234567890", user.PhoneNumber);
        }

        [Fact]
        public void User_Groups_DefaultsToEmptyList()
        {
            var user = new User();

            Assert.NotNull(user.Groups);
            Assert.Empty(user.Groups);
        }

        [Fact]
        public void User_Groups_CanBeAssignedAndRetrieved()
        {
            var groups = new List<Group>
            {
                new Group { Id = 1, Name = "Development" },
                new Group { Id = 2, Name = "Design" }
            };

            var user = new User { Groups = groups };

            Assert.NotNull(user.Groups);
            Assert.Equal(2, user.Groups.Count);
            Assert.Contains(user.Groups, g => g.Name == "Development");
            Assert.Contains(user.Groups, g => g.Name == "Design");
        }

        [Fact]
        public void User_DefaultConstructor_InitializesObject()
        {
            var user = new User();
            Assert.NotNull(user);
        }
    }
}