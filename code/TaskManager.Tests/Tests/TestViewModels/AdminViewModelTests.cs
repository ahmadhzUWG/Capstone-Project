using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class AdminViewModelTests
    {
        [Fact]
        public void TestAdminViewModel()
        {
            var vm = new AdminViewModel
            {
                UserName = "testUser",
                Email = "user@gmail.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            Assert.Equal("testUser", vm.UserName);
            Assert.Equal("user@gmail.com", vm.Email);
            Assert.Equal("password123", vm.Password);
            Assert.Equal("password123", vm.ConfirmPassword);
        }
    }
}