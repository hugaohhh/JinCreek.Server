using System;
using Admin;
using Admin.Controllers;
using Admin.CustomProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Moq;
using Xunit;

namespace AdminTests.UnitTests.Controllers
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
            var testUser = new ApplicationUser { UserName = "hoge", Role = "SuperAdminUser" };

            var userManager = new Mock<FakeUserManager>();
            userManager.Setup(_ => _.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(_ => _.FindByNameAsync(testUser.UserName)).ReturnsAsync(testUser);
            userManager.Setup(_ => _.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(testUser);

            var signInManager = new Mock<FakeSignInManager>();
            signInManager.Setup(_ => _.CheckPasswordSignInAsync(testUser, "fuga", true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

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
        public class FakeSignInManager : SignInManager<ApplicationUser>
        {
            public FakeSignInManager() : base(
                new FakeUserManager(),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object)
            { }
        }

        public class FakeUserManager : UserManager<ApplicationUser>
        {
            public FakeUserManager() : base(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object)
            { }
        }
    }
}
