using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Xunit;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class CreateStageViewModelTests
    {
        [Fact]
        public void DefaultConstructor_PropertiesAreInitialized()
        {
            var vm = new CreateStageViewModel();

            Assert.Null(vm.Name);
            Assert.Equal(0, vm.Position);
            Assert.Null(vm.SelectedGroupId);
            Assert.NotNull(vm.AvailableGroups);
            Assert.Empty(vm.AvailableGroups);
        }

        [Fact]
        public void Name_CanBeSetAndRetrieved()
        {
            var vm = new CreateStageViewModel { Name = "Test Stage" };
            Assert.Equal("Test Stage", vm.Name);
        }

        [Fact]
        public void Position_CanBeSetAndRetrieved()
        {
            var vm = new CreateStageViewModel { Position = 5 };
            Assert.Equal(5, vm.Position);
        }

        [Fact]
        public void SelectedGroupId_CanBeSetAndRetrieved()
        {
            var vm = new CreateStageViewModel { SelectedGroupId = 10 };
            Assert.Equal(10, vm.SelectedGroupId);
        }

        [Fact]
        public void AvailableGroups_CanBeSetAndRetrieved()
        {
            var vm = new CreateStageViewModel();
            var groups = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Group A" },
                new SelectListItem { Value = "2", Text = "Group B" }
            };

            vm.AvailableGroups = groups;

            Assert.NotNull(vm.AvailableGroups);
            Assert.Equal(2, vm.AvailableGroups.Count());
            Assert.Contains(vm.AvailableGroups, g => g.Value == "1" && g.Text == "Group A");
            Assert.Contains(vm.AvailableGroups, g => g.Value == "2" && g.Text == "Group B");
        }

        [Fact]
        public void Name_ValidationFailsWhenMissing()
        {
            var vm = new CreateStageViewModel
            {
                Name = null,
                Position = 1
            };

            var results = ValidateModel(vm);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(vm.Name)));
        }

        [Fact]
        public void Position_ValidationFailsWhenZero()
        {
            var vm = new CreateStageViewModel
            {
                Name = "Stage",
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