using TaskManagerWebsite.ViewModels;

namespace TaskManager.Tests.WebsiteTests.TestViewModels
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
