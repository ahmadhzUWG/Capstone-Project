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

            Assert.NotNull(user.UserGroups);
            Assert.Empty(user.UserGroups);
        }

        [Fact]
        public void User_UserGroups_CanBeAssignedAndRetrieved()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { Id = 1, Name = "Development" },
                new Group { Id = 2, Name = "Design" }
            };

            var user = new User
            {
                UserGroups = new List<UserGroup>
                {
                    new UserGroup { GroupId = 1, Group = groups[0], Role = "Member" },
                    new UserGroup { GroupId = 2, Group = groups[1], Role = "Manager" }
                }
            };

            // Act & Assert
            Assert.NotNull(user.UserGroups);
            Assert.Equal(2, user.UserGroups.Count);
            Assert.Contains(user.UserGroups, ug => ug.Group.Name == "Development");
            Assert.Contains(user.UserGroups, ug => ug.Group.Name == "Design");
        }

        [Fact]
        public void User_DefaultConstructor_InitializesObject()
        {
            var user = new User();
            Assert.NotNull(user);
        }
    }
}