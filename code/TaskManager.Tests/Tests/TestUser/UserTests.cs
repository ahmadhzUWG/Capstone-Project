using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestUser
{
    public class UserTests
    {
        [Fact]
        public void TestUser()
        {
            var user = new User
            {
                UserName = "testUser",
                Email = "test@example.com",
                Role = "User"
            };

            Assert.Equal("testUser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("User", user.Role);
        }
    }
}
