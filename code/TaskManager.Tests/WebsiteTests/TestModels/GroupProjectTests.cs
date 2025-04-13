using TaskManagerData.Models;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class GroupProjectTests
    {
        [Fact]
        public void GroupProject_ProjectId_CanBeSetAndRetrieved()
        {
            var groupProject = new GroupProject { ProjectId = 10 };
            Assert.Equal(10, groupProject.ProjectId);
        }

        [Fact]
        public void GroupProject_Project_CanBeAssignedAndRetrieved()
        {
            var project = new Project { Id = 1, Name = "New Website" };
            var groupProject = new GroupProject { Project = project };

            Assert.NotNull(groupProject.Project);
            Assert.Equal(1, groupProject.Project.Id);
            Assert.Equal("New Website", groupProject.Project.Name);
        }

        [Fact]
        public void GroupProject_GroupId_CanBeSetAndRetrieved()
        {
            var groupProject = new GroupProject { GroupId = 20 };
            Assert.Equal(20, groupProject.GroupId);
        }

        [Fact]
        public void GroupProject_Group_CanBeAssignedAndRetrieved()
        {
            var group = new Group { Id = 2, Name = "Development Team" };
            var groupProject = new GroupProject { Group = group };

            Assert.NotNull(groupProject.Group);
            Assert.Equal(2, groupProject.Group.Id);
            Assert.Equal("Development Team", groupProject.Group.Name);
        }

        [Fact]
        public void GroupProject_CanBeInstantiatedWithValues()
        {
            var project = new Project { Id = 5, Name = "API Development" };
            var group = new Group { Id = 3, Name = "Backend Team" };

            var groupProject = new GroupProject
            {
                ProjectId = 5,
                Project = project,
                GroupId = 3,
                Group = group
            };

            Assert.Equal(5, groupProject.ProjectId);
            Assert.Equal(3, groupProject.GroupId);
            Assert.Equal("API Development", groupProject.Project.Name);
            Assert.Equal("Backend Team", groupProject.Group.Name);
        }
    }
}
