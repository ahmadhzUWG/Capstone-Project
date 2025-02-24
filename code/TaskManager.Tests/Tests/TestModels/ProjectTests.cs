using System.ComponentModel.DataAnnotations;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestModels
{
    public class ProjectTests
    {
        [Fact]
        public void Project_Id_CanBeSetAndRetrieved()
        {
            var project = new Project { Id = 1 };
            Assert.Equal(1, project.Id);
        }

        [Fact]
        public void Project_Name_CanBeSetAndRetrieved()
        {
            var project = new Project { Name = "New Application" };
            Assert.Equal("New Application", project.Name);
        }

        [Fact]
        public void Project_Description_CanBeSetAndRetrieved()
        {
            var project = new Project { Description = "Developing a new application" };
            Assert.Equal("Developing a new application", project.Description);
        }

        [Fact]
        public void Project_ProjectLeadId_CanBeSetAndRetrieved()
        {
            var project = new Project { ProjectLeadId = 5 };
            Assert.Equal(5, project.ProjectLeadId);
        }

        [Fact]
        public void Project_ProjectLead_CanBeAssignedAndRetrieved()
        {
            var user = new User { Id = 2, UserName = "LeadUser" };
            var project = new Project { ProjectLead = user };

            Assert.NotNull(project.ProjectLead);
            Assert.Equal(2, project.ProjectLead.Id);
            Assert.Equal("LeadUser", project.ProjectLead.UserName);
        }

        [Fact]
        public void Project_ProjectCreatorId_CanBeSetAndRetrieved()
        {
            var project = new Project { ProjectCreatorId = 10 };
            Assert.Equal(10, project.ProjectCreatorId);
        }

        [Fact]
        public void Project_ProjectGroups_DefaultsToEmptyList()
        {
            var project = new Project();
            Assert.NotNull(project.ProjectGroups);
            Assert.Empty(project.ProjectGroups);
        }

        [Fact]
        public void Project_ProjectGroups_CanBeAssignedAndRetrieved()
        {
            var groups = new List<GroupProject>
            {
                new GroupProject { ProjectId = 1, GroupId = 2 },
                new GroupProject { ProjectId = 1, GroupId = 3 }
            };

            var project = new Project { ProjectGroups = groups };

            Assert.NotNull(project.ProjectGroups);
            Assert.Equal(2, project.ProjectGroups.Count);
        }

        [Fact]
        public void Project_Name_ValidationFails_WhenNull()
        {
            var project = new Project { Name = null };
            var validationResults = ValidateModel(project);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Project_Description_ValidationFails_WhenNull()
        {
            var project = new Project { Description = null };
            var validationResults = ValidateModel(project);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }

        [Fact]
        public void Project_Name_ValidationFails_WhenExceedsMaxLength()
        {
            var project = new Project { Name = new string('A', 101) };
            var validationResults = ValidateModel(project);

            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Project_Description_ValidationFails_WhenExceedsMaxLength()
        {
            var project = new Project { Description = new string('B', 501) };
            var validationResults = ValidateModel(project);

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
