using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskManager.Controllers;
using TaskManagerWebsite.Models;

namespace TaskManager.Tests.Tests.TestHomeController
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var controller = new HomeController(new Logger<HomeController>(new LoggerFactory()));

            var result = controller.Index();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var controller = new HomeController(new Logger<HomeController>(new LoggerFactory()));

            var result = controller.Privacy();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult()
        {
            var dbContext = TestHelper.GetDbContext();
            var controller = new HomeController(new Logger<HomeController>(new LoggerFactory()));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }
    }
}
