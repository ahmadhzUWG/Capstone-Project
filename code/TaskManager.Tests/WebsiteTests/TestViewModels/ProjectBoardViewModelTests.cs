using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Xunit;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class ProjectBoardViewModelTests
    {
        [Fact]
        public void DefaultConstructor_PropertiesAreNullOrDefault()
        {
            var vm = new ProjectBoardViewModel();

            Assert.Null(vm.Project);
            Assert.Null(vm.StageForm);
            Assert.False(vm.CanAddStage);
        }

        [Fact]
        public void Project_CanBeSetAndRetrieved()
        {
            var vm = new ProjectBoardViewModel();
            var project = new Project { Id = 123, Name = "Test Project" };

            vm.Project = project;

            Assert.NotNull(vm.Project);
            Assert.Equal(123, vm.Project.Id);
            Assert.Equal("Test Project", vm.Project.Name);
        }

        [Fact]
        public void StageForm_CanBeSetAndRetrieved()
        {
            var vm = new ProjectBoardViewModel();
            var form = new CreateStageViewModel { Name = "New Stage" };

            vm.StageForm = form;

            Assert.NotNull(vm.StageForm);
            Assert.Equal("New Stage", vm.StageForm.Name);
        }

        [Fact]
        public void CanAddStage_CanBeSetAndRetrieved()
        {
            var vm = new ProjectBoardViewModel();

            vm.CanAddStage = true;
            Assert.True(vm.CanAddStage);

            vm.CanAddStage = false;
            Assert.False(vm.CanAddStage);
        }
    }
}