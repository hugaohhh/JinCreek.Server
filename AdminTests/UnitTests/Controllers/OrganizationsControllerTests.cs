﻿using Admin.Controllers;
using Admin.Data;
using Admin.Models;
using Admin.Services;
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
            Assert.Equal(org, (controller.GetOrganization(org.Id)).Value);
            ActionResult<Organization> hoge = controller.GetOrganization("6");
            Assert.IsType<NotFoundResult>(hoge.Result);

            // DELETE: api/Organizations/5
            Assert.Equal(org, (controller.DeleteOrganization(org.Id)).Value);
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