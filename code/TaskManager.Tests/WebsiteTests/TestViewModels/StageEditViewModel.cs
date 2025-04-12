using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagerData.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;

namespace TaskManager.Tests.WebsiteTests.TestViewModels
{
    public class StageEditViewModelTests
    {
        [Fact]
        public void DefaultConstructor_PropertiesAreInitialized()
        {
            var vm = new StageEditViewModel();

            Assert.Equal(0, vm.StageId);
            Assert.Equal(0, vm.ProjectId);
            Assert.Null(vm.Name);
            Assert.Equal(0, vm.Position);
            Assert.Null(vm.SelectedGroupId);
            Assert.NotNull(vm.AvailableGroups);
            Assert.Empty(vm.AvailableGroups);
            Assert.NotNull(vm.AllStages);
            Assert.Empty(vm.AllStages);
        }

        [Fact]
        public void StageId_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel { StageId = 123 };
            Assert.Equal(123, vm.StageId);
        }

        [Fact]
        public void ProjectId_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel { ProjectId = 999 };
            Assert.Equal(999, vm.ProjectId);
        }

        [Fact]
        public void Name_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel { Name = "Edit Stage" };
            Assert.Equal("Edit Stage", vm.Name);
        }

        [Fact]
        public void Position_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel { Position = 10 };
            Assert.Equal(10, vm.Position);
        }

        [Fact]
        public void SelectedGroupId_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel { SelectedGroupId = 5 };
            Assert.Equal(5, vm.SelectedGroupId);
        }

        [Fact]
        public void AvailableGroups_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel();
            var groups = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Group X" },
                new SelectListItem { Value = "2", Text = "Group Y" }
            };

            vm.AvailableGroups = groups;

            Assert.NotNull(vm.AvailableGroups);
            Assert.Equal(2, vm.AvailableGroups.Count);
            Assert.Contains(vm.AvailableGroups, g => g.Value == "1" && g.Text == "Group X");
        }

        [Fact]
        public void AllStages_CanBeSetAndRetrieved()
        {
            var vm = new StageEditViewModel();
            var stages = new List<Stage>
            {
                new Stage { Id = 1, Name = "Stage A" },
                new Stage { Id = 2, Name = "Stage B" }
            };

            vm.AllStages = stages;

            Assert.NotNull(vm.AllStages);
            Assert.Equal(2, vm.AllStages.Count);
            Assert.Contains(vm.AllStages, s => s.Id == 1 && s.Name == "Stage A");
        }

        [Fact]
        public void Name_ValidationFailsWhenMissing()
        {
            var vm = new StageEditViewModel
            {
                StageId = 1,
                ProjectId = 100,
                Name = null,
                Position = 5
            };

            var results = ValidateModel(vm);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(vm.Name)));
        }

        [Fact]
        public void Position_ValidationFailsWhenZero()
        {
            var vm = new StageEditViewModel
            {
                StageId = 2,
                ProjectId = 200,
                Name = "Valid Stage",
                Position = 0
            };

            var results = ValidateModel(vm);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(vm.Position)));
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}