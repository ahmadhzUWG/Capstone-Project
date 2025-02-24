using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class GroupRequestTests
    {
        [Fact]
        public void GroupRequest_Id_CanBeSetAndRetrieved()
        {
            var request = new GroupRequest { Id = 1 };
            Assert.Equal(1, request.Id);
        }

        [Fact]
        public void GroupRequest_SenderId_CanBeSetAndRetrieved()
        {
            var request = new GroupRequest { SenderId = 100 };
            Assert.Equal(100, request.SenderId);
        }

        [Fact]
        public void GroupRequest_GroupId_CanBeSetAndRetrieved()
        {
            var request = new GroupRequest { GroupId = 50 };
            Assert.Equal(50, request.GroupId);
        }

        [Fact]
        public void GroupRequest_Group_CanBeAssignedAndRetrieved()
        {
            var group = new Group { Id = 5, Name = "Development Team" };
            var request = new GroupRequest { Group = group };

            Assert.NotNull(request.Group);
            Assert.Equal(5, request.Group.Id);
            Assert.Equal("Development Team", request.Group.Name);
        }

        [Fact]
        public void GroupRequest_ProjectId_CanBeSetAndRetrieved()
        {
            var request = new GroupRequest { ProjectId = 25 };
            Assert.Equal(25, request.ProjectId);
        }

        [Fact]
        public void GroupRequest_Project_CanBeAssignedAndRetrieved()
        {
            var project = new Project { Id = 10, Name = "New App" };
            var request = new GroupRequest { Project = project };

            Assert.NotNull(request.Project);
            Assert.Equal(10, request.Project.Id);
            Assert.Equal("New App", request.Project.Name);
        }

        [Fact]
        public void GroupRequest_Response_CanBeSetToTrue()
        {
            var request = new GroupRequest { Response = true };
            Assert.True(request.Response);
        }

        [Fact]
        public void GroupRequest_Response_CanBeSetToFalse()
        {
            var request = new GroupRequest { Response = false };
            Assert.False(request.Response);
        }

        [Fact]
        public void GroupRequest_Response_CanBeSetToNull()
        {
            var request = new GroupRequest { Response = null };
            Assert.Null(request.Response);
        }

        [Fact]
        public void GroupRequest_CanBeInstantiatedWithValues()
        {
            var group = new Group { Id = 3, Name = "Marketing Team" };
            var project = new Project { Id = 7, Name = "Website Revamp" };

            var request = new GroupRequest
            {
                Id = 1,
                SenderId = 200,
                GroupId = 3,
                Group = group,
                ProjectId = 7,
                Project = project,
                Response = null
            };

            Assert.Equal(1, request.Id);
            Assert.Equal(200, request.SenderId);
            Assert.Equal(3, request.GroupId);
            Assert.Equal(7, request.ProjectId);
            Assert.Null(request.Response);
            Assert.Equal("Marketing Team", request.Group.Name);
            Assert.Equal("Website Revamp", request.Project.Name);
        }
    }
}
