using Xunit;
using TaskManagerData.Models;

namespace TaskManager.Tests.Models
{
    public class UserOptionTests
    {
        [Fact]
        public void DisplayName_Returns_NA_WhenUserIsNull()
        {
            // Arrange
            var userOption = new UserOption { User = null };

            // Act
            var displayName = userOption.DisplayName;

            // Assert
            Assert.Equal("N/A", displayName);
        }

        [Fact]
        public void DisplayName_Returns_UserName_WhenUserIsProvided()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "Alice" };
            var userOption = new UserOption { User = user };

            // Act
            var displayName = userOption.DisplayName;

            // Assert
            Assert.Equal("Alice", displayName);
        }

        [Fact]
        public void ToString_Returns_SameValue_As_DisplayName()
        {
            // Arrange
            var user = new User { Id = 2, UserName = "Bob" };
            var userOption = new UserOption { User = user };

            // Act
            var toStringResult = userOption.ToString();
            var displayName = userOption.DisplayName;

            // Assert
            Assert.Equal(displayName, toStringResult);
        }
    }
}