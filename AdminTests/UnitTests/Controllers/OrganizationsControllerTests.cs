using Admin.Controllers;
using Admin.Services;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace AdminTests.UnitTests.Controllers
{
    public class OrganizationsControllerTests
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationRepository _organizations;

        public OrganizationsControllerTests()
        {
            var services = new ServiceCollection();
            services.AddAuthorization();
            services.AddLogging();
            services.AddOptions();
            services.AddSingleton<IAuthorizationHandler, OrganizationAuthorizationHandler>();
            _authorizationService = services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
            _organizations = new OrganizationRepository(CreateInMemoryDatabaseContext());
        }

        [Fact]
        public void TestCrud()
        {
            var controller = new OrganizationsController(_authorizationService, _organizations);
            var org = new Organization
            {
                Id = new Guid(),
                Code = "1",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                IsValid = true,
            };

            {
                // POST: api/Organizations
                ActionResult<Organization> result = controller.PostOrganization(org);
                Assert.IsType<CreatedAtActionResult>(result.Result);
                Assert.Equal(org, ((CreatedAtActionResult)result.Result).Value);
            }

            // PUT: api/Organizations/5
            Assert.IsType<NoContentResult>(controller.PutOrganization(org.Id, org));
            Assert.IsType<BadRequestResult>(controller.PutOrganization(new Guid(), org));

            {
                // GET: api/Organizations
                IEnumerable<Organization> result = controller.GetOrganizations();
                Assert.IsType<List<Organization>>(result);
                Assert.Equal(org, ((List<Organization>)result)[0]);
            }

            // GET: api/Organizations/5
            Assert.Equal(org, (controller.GetOrganization(org.Id)).Value);
            ActionResult<Organization> hoge = controller.GetOrganization(new Guid());
            Assert.IsType<NotFoundResult>(hoge.Result);

            // DELETE: api/Organizations/5
            Assert.Equal(org, (controller.DeleteOrganization(org.Id)).Value);
            Assert.IsType<NotFoundResult>((controller.DeleteOrganization(new Guid())).Result);


            //// why?
            //var org5 = new Organization { Id = "5" };
            //var org6 = new Organization { Id = "6" };
            //await controller.PostOrganization(org5);
            //await controller.PutOrganization(org6.Id, org6);
            //await controller.DeleteOrganization(org5.Id);  // => Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException : Attempted to update or delete an entity that does not exist in the store.

        }

        private static MainDbContext CreateInMemoryDatabaseContext()
        {
            var context = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
