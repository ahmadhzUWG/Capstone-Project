using TaskManagerData.Models;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class StageTests
    {
        [Fact]
        public void Stage_Id_CanBeSetAndRetrieved()
        {
            var stage = new Stage { Id = 1 };
            Assert.Equal(1, stage.Id);
        }

        [Fact]
        public void Stage_Name_CanBeSetAndRetrieved()
        {
            var stage = new Stage { Name = "In Progress" };
            Assert.Equal("In Progress", stage.Name);
        }

        [Fact]
        public void Stage_Position_CanBeSetAndRetrieved()
        {
            var stage = new Stage { Position = 2 };
            Assert.Equal(2, stage.Position);
        }

        [Fact]
        public void Stage_ProjectBoardId_CanBeSetAndRetrieved()
        {
            var stage = new Stage { ProjectBoardId = 10 };
            Assert.Equal(10, stage.ProjectBoardId);
        }

        [Fact]
        public void Stage_ProjectBoard_CanBeAssignedAndRetrieved()
        {
            var board = new ProjectBoard { Id = 5, BoardCreatorId = 1, ProjectId = 15 };
            var stage = new Stage { ProjectBoard = board };
            Assert.NotNull(stage.ProjectBoard);
            Assert.Equal(5, stage.ProjectBoard.Id);
        }

        [Fact]
        public void Stage_CreatorGroupId_CanBeSetAndRetrieved()
        {
            var stage = new Stage { CreatorGroupId = 3 };
            Assert.Equal(3, stage.CreatorGroupId);
        }

        [Fact]
        public void Stage_CreatorGroup_CanBeAssignedAndRetrieved()
        {
            var group = new Group { Id = 7, Name = "Development", Description = "Dev Group" };
            var stage = new Stage { CreatorGroup = group };
            Assert.NotNull(stage.CreatorGroup);
            Assert.Equal(7, stage.CreatorGroup.Id);
            Assert.Equal("Development", stage.CreatorGroup.Name);
        }

        [Fact]
        public void Stage_CreatorUserId_CanBeSetAndRetrieved()
        {
            var stage = new Stage { CreatorUserId = 4 };
            Assert.Equal(4, stage.CreatorUserId);
        }

        [Fact]
        public void Stage_CreatorUser_CanBeAssignedAndRetrieved()
        {
            var user = new User { Id = 9, UserName = "CreatorUser" };
            var stage = new Stage { CreatorUser = user };
            Assert.NotNull(stage.CreatorUser);
            Assert.Equal(9, stage.CreatorUser.Id);
            Assert.Equal("CreatorUser", stage.CreatorUser.UserName);
        }

        [Fact]
        public void Stage_AssignedGroupId_CanBeSetAndRetrieved()
        {
            var stage = new Stage { AssignedGroupId = 8 };
            Assert.Equal(8, stage.AssignedGroupId);
        }

        [Fact]
        public void Stage_AssignedGroup_CanBeAssignedAndRetrieved()
        {
            var group = new Group { Id = 11, Name = "QA", Description = "Quality Assurance" };
            var stage = new Stage { AssignedGroup = group };
            Assert.NotNull(stage.AssignedGroup);
            Assert.Equal(11, stage.AssignedGroup.Id);
            Assert.Equal("QA", stage.AssignedGroup.Name);
        }

    }
}
