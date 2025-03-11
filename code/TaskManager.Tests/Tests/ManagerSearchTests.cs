using Bunit;
using Xunit;
using System.Collections.Generic;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.Components;
using Microsoft.AspNetCore.Components;
using Org.BouncyCastle.Tls;

namespace TaskManagerWebsite.Tests
{
    public class ManagerSearchTests : TestContext
    {
        [Fact]
        public void RendersAllManagersWhenSearchIsEmpty()
        {
            // Arrange
            var managers = new List<User>
            {
                new User { Id = 1, UserName = "Alice", Email = "alice@example.com" },
                new User { Id = 2, UserName = "Bob", Email = "bob@example.com" }
            };

            // Act: Render the component with no selected manager.
            var cut = RenderComponent<ManagerSearch>(parameters => parameters
                .Add(p => p.Managers, managers)
                .Add(p => p.SelectedManagerId, (int?)null)
            );

            // Assert: There should be the default option plus an option for each manager.
            var options = cut.FindAll("select option");
            Assert.Equal(managers.Count + 1, options.Count);
            Assert.Contains(options, option => option.TextContent.Contains("Alice"));
            Assert.Contains(options, option => option.TextContent.Contains("Bob"));
        }

        [Fact]
        public void FiltersManagersBasedOnSearchText()
        {
            // Arrange
            var managers = new List<User>
            {
                new User { Id = 1, UserName = "Alice", Email = "alice@example.com" },
                new User { Id = 2, UserName = "Bob", Email = "bob@example.com" }
            };

            var cut = RenderComponent<ManagerSearch>(parameters => parameters
                .Add(p => p.Managers, managers)
                .Add(p => p.SelectedManagerId, (int?)null)
            );

            // Act: Simulate typing "alice" in the search input.
            var searchInput = cut.Find("input[type='text']");
            searchInput.Input("alice");

            // Assert: The select should now have only the default option and one matching option.
            var options = cut.FindAll("select option");
            Assert.Equal(2, options.Count); // default option + "Alice" option
            Assert.Contains(options, option => option.TextContent.Contains("Alice"));
            Assert.DoesNotContain(options, option => option.TextContent.Contains("Bob"));
        }

        [Fact]
        public void InvokesSelectedManagerIdChangedWhenSelectionChanges()
        {
            // Arrange
            var managers = new List<User>
            {
                new User { Id = 1, UserName = "Alice", Email = "alice@example.com" },
                new User { Id = 2, UserName = "Bob", Email = "bob@example.com" }
            };

            int? callbackValue = null;
            var cut = RenderComponent<ManagerSearch>(parameters => parameters
                .Add(p => p.Managers, managers)
                .Add(p => p.SelectedManagerId, (int?)null)
                .Add<EventCallback<int?>>(p => p.SelectedManagerIdChanged, EventCallback.Factory.Create<int?>(this, val => callbackValue = val))
            );

            // Act: Change the selection to manager with Id "2".
            var select = cut.Find("select");
            select.Change("2");

            // Assert: The callback should be triggered with value 2.
            Assert.Equal(2, callbackValue);
        }
    }
}
