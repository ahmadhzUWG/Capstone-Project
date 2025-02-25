using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.Tests.TestViewModels
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void TestErrorViewModel()
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();
            errorViewModel.RequestId = "123";
            Assert.True(errorViewModel.ShowRequestId);
        }
    }
}
