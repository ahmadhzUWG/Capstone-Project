using TaskManagerData.Models;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class ProjectBoardTests
    {
        [Fact]
        public void ProjectBoard_Id_CanBeSetAndRetrieved()
        {
            var board = new ProjectBoard { Id = 1 };
            Assert.Equal(1, board.Id);
        }

        [Fact]
        public void ProjectBoard_ProjectId_CanBeSetAndRetrieved()
        {
            var board = new ProjectBoard { ProjectId = 10 };
            Assert.Equal(10, board.ProjectId);
        }

        [Fact]
        public void ProjectBoard_BoardCreatorId_CanBeSetAndRetrieved()
        {
            var board = new ProjectBoard { BoardCreatorId = 5 };
            Assert.Equal(5, board.BoardCreatorId);
        }

        [Fact]
        public void ProjectBoard_Project_CanBeAssignedAndRetrieved()
        {
            var project = new Project { Id = 2, Name = "Test Project" };
            var board = new ProjectBoard { Project = project };
            Assert.NotNull(board.Project);
            Assert.Equal(2, board.Project.Id);
            Assert.Equal("Test Project", board.Project.Name);
        }

        [Fact]
        public void ProjectBoard_BoardCreator_CanBeAssignedAndRetrieved()
        {
            var user = new User { Id = 3, UserName = "BoardCreator" };
            var board = new ProjectBoard { BoardCreator = user };
            Assert.NotNull(board.BoardCreator);
            Assert.Equal(3, board.BoardCreator.Id);
            Assert.Equal("BoardCreator", board.BoardCreator.UserName);
        }

        [Fact]
        public void ProjectBoard_Stages_DefaultsToNull()
        {
            var board = new ProjectBoard();
            Assert.Null(board.Stages);
        }

        [Fact]
        public void ProjectBoard_Stages_CanBeAssignedAndRetrieved()
        {
            var stages = new List<Stage>
            {
                new Stage { Id = 1, Name = "To Do" },
                new Stage { Id = 2, Name = "Done" }
            };

            var board = new ProjectBoard { Stages = stages };
            Assert.NotNull(board.Stages);
            Assert.Equal(2, board.Stages.Count);
            Assert.Contains(board.Stages, s => s.Name == "To Do");
            Assert.Contains(board.Stages, s => s.Name == "Done");
        }

    }
}
