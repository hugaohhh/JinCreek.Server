using Admin.Controllers;
using Xunit;

namespace AdminTests.UnitTests.Controllers
{
    public class RandomNumberControllerTests
    {
        [Fact]
        public void GetDeviceAuthenticationTest()
        {
            var controller = new RandomNumberController();
            Assert.InRange<int>(controller.Generate().Value, 0, 1000);
        }
    }
}
