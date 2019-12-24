using Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AdminTests.Controllers
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
