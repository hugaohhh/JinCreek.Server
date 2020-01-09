using System;
using System.Collections.Generic;
using Admin.Controllers;
using Admin.Data;
using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AdminTests.UnitTests.Controllers
{
    public class OrganizationsControllerTests
    {
        private readonly IOrganizationRepository _organizations;

        public OrganizationsControllerTests()
        {
            _organizations = new OrganizationRepository(CreateInMemoryDatabaseContext());
        }

        [Fact]
        public void TestCrud()
        {
            var controller = new OrganizationsController(_organizations);
            var org = new Organization { Id = "5" };

            {
                // POST: api/Organizations
                ActionResult<Organization> result = controller.PostOrganization(org);
                Assert.IsType<CreatedAtActionResult>(result.Result);
                Assert.Equal(org, ((CreatedAtActionResult)result.Result).Value);
            }

            // PUT: api/Organizations/5
            Assert.IsType<NoContentResult>(controller.PutOrganization(org.Id, org));
            Assert.IsType<BadRequestResult>(controller.PutOrganization("6", org));

            {
                // GET: api/Organizations
                IEnumerable<Organization> result = controller.GetOrganizations();
                Assert.IsType<List<Organization>>(result);
                Assert.Equal(org, ((List<Organization>)result)[0]);
            }

            // GET: api/Organizations/5
            Assert.Equal(org, (controller.GetOrganization("5")).Value);
            ActionResult<Organization> hoge = controller.GetOrganization("6");
            Assert.IsType<NotFoundResult>(hoge.Result);

            // DELETE: api/Organizations/5
            Assert.Equal(org, (controller.DeleteOrganization("5")).Value);
            Assert.IsType<NotFoundResult>((controller.DeleteOrganization("6")).Result);


            //// why?
            //var org5 = new Organization { Id = "5" };
            //var org6 = new Organization { Id = "6" };
            //await controller.PostOrganization(org5);
            //await controller.PutOrganization(org6.Id, org6);
            //await controller.DeleteOrganization(org5.Id);  // => Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException : Attempted to update or delete an entity that does not exist in the store.

        }

        private static ApplicationDbContext CreateInMemoryDatabaseContext()
        {
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
