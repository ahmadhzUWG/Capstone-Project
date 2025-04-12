using TaskManagerData.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.WebsiteTests.TestViewModels
{
    public class UserDeleteViewModelTests
    {
        [Fact]
        public void DefaultConstructor_InitializesPropertiesAsNull()
        {
            var viewModel = new UserDeleteViewModel
            {
                User = null,
                RelatedGroups = null,
                RelatedProjects = null
            };

            Assert.Null(viewModel.User);
            Assert.Null(viewModel.RelatedGroups);
            Assert.Null(viewModel.RelatedProjects);
        }

        [Fact]
        public void SetAndGet_User_WorksCorrectly()
        {
            var viewModel = new UserDeleteViewModel
            {
                User = new User { Id = 1, UserName = string.Empty },
                RelatedGroups = null,
                RelatedProjects = null
            };

            var user = new User { Id = 2, UserName = "TestUser" };
            viewModel.User = user;

            Assert.NotNull(viewModel.User);
            Assert.Equal(2, viewModel.User.Id);
            Assert.Equal("TestUser", viewModel.User.UserName);
        }

        [Fact]
        public void SetAndGet_RelatedGroups_WorksCorrectly()
        {
            var groups = new List<Group>
            {
                new Group { Id = 10, Name = "GroupA" },
                new Group { Id = 11, Name = "GroupB" }
            };

            var viewModel = new UserDeleteViewModel
            {
                User = null,
                RelatedGroups = groups,
                RelatedProjects = null
            };

            Assert.NotNull(viewModel.RelatedGroups);
            Assert.Equal(2, viewModel.RelatedGroups.Count);
            Assert.Contains(viewModel.RelatedGroups, g => g.Id == 10 && g.Name == "GroupA");
            Assert.Contains(viewModel.RelatedGroups, g => g.Id == 11 && g.Name == "GroupB");
        }

        [Fact]
        public void SetAndGet_RelatedProjects_WorksCorrectly()
        {
            var projects = new List<Project>
            {
                new Project { Id = 100, Name = "ProjectA" },
                new Project { Id = 101, Name = "ProjectB" }
            };

            var viewModel = new UserDeleteViewModel
            {
                User = null,
                RelatedGroups = null,
                RelatedProjects = projects
            };

            Assert.NotNull(viewModel.RelatedProjects);
            Assert.Equal(2, viewModel.RelatedProjects.Count);
            Assert.Contains(viewModel.RelatedProjects, p => p.Id == 100 && p.Name == "ProjectA");
            Assert.Contains(viewModel.RelatedProjects, p => p.Id == 101 && p.Name == "ProjectB");
        }
    }
}
