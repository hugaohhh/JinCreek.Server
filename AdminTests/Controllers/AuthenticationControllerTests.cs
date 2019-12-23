using Admin;
using Admin.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdminTests.Controllers
{
    [TestClass]
    public class AuthenticationControllerTests
    {
        [TestMethod]
        public void TestAuthentication()
        {
            IdentityModelEventSource.ShowPII = true;  // see https://aka.ms/IdentityModel/PII

            var userManagerMock = new Mock<FakeUserManager>();
            userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var signInManagerMock = new Mock<FakeSignInManager>();
            signInManagerMock.Setup(manager => manager.PasswordSignInAsync("hoge", "fuga", true, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var optionsMock = new Mock<IOptions<AppSettings>>();
            optionsMock.Setup(options => options.Value).Returns(new AppSettings { Secret = "this is my key, there are many of them but this one is mine" });

            var controller = new AuthenticationController(userManagerMock.Object, signInManagerMock.Object, optionsMock.Object);

            var result1 = controller.Register(new UsersRegisterRequest { UserName = "hoge", Password = "fuga" });
            Assert.IsNotNull(result1.Result);

            var result2 = controller.Login(new UsersLoginRequest { UserName = "hoge", Password = "fuga" });
            Assert.IsNotNull(result2.Result);

            var result3 = controller.Refresh(new UsersRefreshRequest { RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM2OWI5ZTUyLWNkZjItNGE4NS1iZjNiLWRkMDViMWZkNmNiNiIsIm5iZiI6MTU3NjgxOTIyOSwiZXhwIjoxNTc2ODYyNDI5LCJpYXQiOjE1NzY4MTkyMjl9.vIXlAOJgHaPjug4pk6lMwjq8myaFWfA-TJT9VrXVFQo" });
            Assert.IsNotNull(result3.Result);
        }

        //
        // see https://github.com/aspnet/Identity/issues/640
        //
        public class FakeSignInManager : SignInManager<IdentityUser>
        {
            public FakeSignInManager() : base(
                new FakeUserManager(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<IdentityUser>>().Object)
            { }
        }

        public class FakeUserManager : UserManager<IdentityUser>
        {
            public FakeUserManager() : base(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object)
            { }
        }
    }
}
