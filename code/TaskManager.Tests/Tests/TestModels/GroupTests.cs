using System.ComponentModel.DataAnnotations;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class GroupTests
    {
        [Fact]
        public void Group_Id_CanBeSetAndRetrieved()
        {
            var group = new Group { Id = 1 };
            Assert.Equal(1, group.Id);
        }

        [Fact]
        public void Group_Name_CanBeSetAndRetrieved()
        {
            var group = new Group { Name = "Development Team" };
            Assert.Equal("Development Team", group.Name);
        }

        [Fact]
        public void Group_Description_CanBeSetAndRetrieved()
        {
            var group = new Group { Description = "A group for all developers" };
            Assert.Equal("A group for all developers", group.Description);
        }

        [Fact]
        public void Group_PrimaryManagerId_CanBeSetAndRetrieved()
        {
            var group = new Group { ManagerId = 5 };
            Assert.Equal(5, group.ManagerId);
        }

        [Fact]
        public void Group_PrimaryManager_CanBeSetAndRetrieved()
        {
            var user = new User { Id = 1, UserName = "AdminUser" };
            var group = new Group { Manager = user };

            Assert.NotNull(group.Manager);
            Assert.Equal("AdminUser", group.Manager.UserName);
        }

        [Fact]
        public void Group_UserGroups_DefaultsToEmptyList()
        {
            // Arrange
            var group = new Group();

            // Act & Assert
            Assert.NotNull(group.UserGroups);
            Assert.Empty(group.UserGroups);
        }


        [Fact]
        public void Group_Managers_DefaultsToEmptyList()
        {
            var group = new Group();
            Assert.NotNull(group.Managers);
            Assert.Empty(group.Managers);
        }

        [Fact]
        public void Group_GroupProjects_DefaultsToEmptyList()
        {
            var group = new Group();
            Assert.NotNull(group.GroupProjects);
            Assert.Empty(group.GroupProjects);
        }

        [Fact]
        public void Group_UserGroups_CanBeAssignedAndRetrieved()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, UserName = "User1" },
                new User { Id = 2, UserName = "User2" }
            };

            var group = new Group
            {
                UserGroups = new List<UserGroup>
                {
                    new UserGroup { UserId = 1, User = users[0], Role = "Member" },
                    new UserGroup { UserId = 2, User = users[1], Role = "Manager" }
                }
            };

            // Act & Assert
            Assert.NotNull(group.UserGroups);
            Assert.Equal(2, group.UserGroups.Count);
            Assert.Contains(group.UserGroups, ug => ug.User.UserName == "User1");
            Assert.Contains(group.UserGroups, ug => ug.User.UserName == "User2");
        }

        [Fact]
        public void Group_Managers_CanBeAssignedAndRetrieved()
        {
            var managers = new List<GroupManager>
            {
                new GroupManager { UserId = 1 },
                new GroupManager { UserId = 2 }
            };

            var group = new Group { Managers = managers };

            Assert.NotNull(group.Managers);
            Assert.Equal(2, group.Managers.Count);
        }

        [Fact]
        public void Group_GroupProjects_CanBeAssignedAndRetrieved()
        {
            var projects = new List<GroupProject>
            {
                new GroupProject { ProjectId = 1 },
                new GroupProject { ProjectId = 2 }
            };

            var group = new Group { GroupProjects = projects };

            Assert.NotNull(group.GroupProjects);
            Assert.Equal(2, group.GroupProjects.Count);
        }

        [Fact]
        public void Group_Name_ValidationFails_WhenNull()
        {
            var group = new Group { Name = null };
            var validationResults = ValidateModel(group);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Group_Description_ValidationFails_WhenNull()
        {
            var group = new Group { Description = null };
            var validationResults = ValidateModel(group);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }

        [Fact]
        public void Group_Name_ValidationFails_WhenExceedsMaxLength()
        {
            var group = new Group { Name = new string('A', 101) };
            var validationResults = ValidateModel(group);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Group_Description_ValidationFails_WhenExceedsMaxLength()
        {
            var group = new Group { Description = new string('B', 501) };
            var validationResults = ValidateModel(group);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
