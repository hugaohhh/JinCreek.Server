using Admin;
using Admin.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AdminTests.Controllers
{
    public class AuthenticationControllerTests
    {

        public AuthenticationControllerTests()
        {
            // see https://aka.ms/IdentityModel/PII
            IdentityModelEventSource.ShowPII = true;
        }

        [Fact]
        public void TestAuthentication()
        {
            static string GetString(OkObjectResult result, string propertyName)
            {
                return result.Value.GetType().GetProperty(propertyName).GetValue(result.Value) as string;
            }


            var testuser = new IdentityUser { UserName = "hoge" };

            var userManager = new Mock<FakeUserManager>();
            userManager.Setup(_ => _.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(_ => _.FindByNameAsync(testuser.UserName)).ReturnsAsync(testuser);
            userManager.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(testuser);

            var signInManager = new Mock<FakeSignInManager>();
            signInManager.Setup(_ => _.PasswordSignInAsync("hoge", "fuga", true, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var appSettings = new Mock<IOptions<AppSettings>>();
            appSettings.Setup(_ => _.Value).Returns(new AppSettings { Secret = "this is my key, there are many of them but this one is mine" });


            var controller = new AuthenticationController(userManager.Object, signInManager.Object, appSettings.Object);

            IActionResult result1 = controller.Register(new UsersRegisterRequest { UserName = "hoge", Password = "fuga" }).Result;
            Assert.IsType<OkResult>(result1);

            IActionResult result2 = controller.Login(new UsersLoginRequest { UserName = "hoge", Password = "fuga" }).Result;
            Assert.IsType<OkObjectResult>(result2);
            string refreshToken = GetString(result2 as OkObjectResult, "RefreshToken");

            IActionResult result3 = controller.Refresh(new UsersRefreshRequest { RefreshToken = refreshToken }).Result;
            Assert.IsType<OkObjectResult>(result3);
            string accessToken = GetString(result3 as OkObjectResult, "AccessToken");
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
