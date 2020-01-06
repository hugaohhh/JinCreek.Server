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
        public async void TestAuthentication()
        {
            static string GetString(OkObjectResult result, string propertyName)
            {
                return (string)result.Value.GetType().GetProperty(propertyName)?.GetValue(result.Value);
            }


            // setup mock
            var testUser = new IdentityUser { UserName = "hoge" };

            var userManager = new Mock<FakeUserManager>();
            userManager.Setup(_ => _.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(_ => _.FindByNameAsync(testUser.UserName)).ReturnsAsync(testUser);
            userManager.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(testUser);

            var signInManager = new Mock<FakeSignInManager>();
            signInManager.Setup(_ => _.PasswordSignInAsync("hoge", "fuga", true, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var appSettings = new Mock<IOptions<AppSettings>>();
            appSettings.Setup(_ => _.Value).Returns(new AppSettings { Secret = "this is my key, there are many of them but this one is mine" });


            // run
            var controller = new AuthenticationController(userManager.Object, signInManager.Object, appSettings.Object);

            IActionResult result1 = await controller.Register(new UsersRegisterRequest { UserName = "hoge", Password = "fuga" });
            Assert.IsType<OkResult>(result1);

            IActionResult result2 = await controller.Login(new UsersLoginRequest { UserName = "hoge", Password = "fuga" });
            Assert.IsType<OkObjectResult>(result2);
            string refreshToken = GetString((OkObjectResult)result2, "RefreshToken");

            IActionResult result3 = await controller.Refresh(new UsersRefreshRequest { RefreshToken = refreshToken });
            Assert.IsType<OkObjectResult>(result3);
            GetString((OkObjectResult)result3, "AccessToken");
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
