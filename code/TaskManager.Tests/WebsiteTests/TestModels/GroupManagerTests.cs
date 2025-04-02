using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class GroupManagerTests
    {
        [Fact]
        public void GroupManager_GroupId_CanBeSetAndRetrieved()
        {
            var groupManager = new GroupManager { GroupId = 15 };
            Assert.Equal(15, groupManager.GroupId);
        }

        [Fact]
        public void GroupManager_Group_CanBeAssignedAndRetrieved()
        {
            var group = new Group { Id = 2, Name = "Engineering Team" };
            var groupManager = new GroupManager { Group = group };

            Assert.NotNull(groupManager.Group);
            Assert.Equal(2, groupManager.Group.Id);
            Assert.Equal("Engineering Team", groupManager.Group.Name);
        }

        [Fact]
        public void GroupManager_UserId_CanBeSetAndRetrieved()
        {
            var groupManager = new GroupManager { UserId = 25 };
            Assert.Equal(25, groupManager.UserId);
        }

        [Fact]
        public void GroupManager_User_CanBeAssignedAndRetrieved()
        {
            var user = new User { Id = 3, UserName = "ManagerUser" };
            var groupManager = new GroupManager { User = user };

            Assert.NotNull(groupManager.User);
            Assert.Equal(3, groupManager.User.Id);
            Assert.Equal("ManagerUser", groupManager.User.UserName);
        }

        [Fact]
        public void GroupManager_CanBeInstantiatedWithValues()
        {
            var group = new Group { Id = 4, Name = "Marketing Team" };
            var user = new User { Id = 7, UserName = "MarketingManager" };

            var groupManager = new GroupManager
            {
                GroupId = 4,
                Group = group,
                UserId = 7,
                User = user
            };

            Assert.Equal(4, groupManager.GroupId);
            Assert.Equal(7, groupManager.UserId);
            Assert.Equal("Marketing Team", groupManager.Group.Name);
            Assert.Equal("MarketingManager", groupManager.User.UserName);
        }
    }
}
