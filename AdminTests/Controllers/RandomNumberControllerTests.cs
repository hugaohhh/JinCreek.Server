using Microsoft.VisualStudio.TestTools.UnitTesting;
using Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdminTests.Controllers
{
    [TestClass]
    public class RandomNumberControllerTests
    {
        [TestMethod]
        public void GetDeviceAuthenticationTest()
        {
            var hoge = new RandomNumberController();

            //Assert.Fail();
            Assert.AreEqual(2, hoge.Generate().Value);

        }
    }
}
