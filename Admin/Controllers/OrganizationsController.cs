using System;
using Admin.Services;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationRepository _organizations;

        public OrganizationsController(IAuthorizationService authorizationService, IOrganizationRepository organizations)
        {
            _authorizationService = authorizationService;
            _organizations = organizations;
        }

        // GET: api/Organizations
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IEnumerable<Organization> GetOrganizations()
        {
            return _organizations.GetAll();
        }

        // GET: api/Organizations/5
        [HttpGet("{id}")]
        public ActionResult<Organization> GetOrganization(Guid id)
        {
            var organization = _organizations.Get(id);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Read).Result.Succeeded)
            {
                return Forbid();
            }
            return organization;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public IActionResult PutOrganization(Guid id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }
            if (_organizations.Get(id) == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Update).Result.Succeeded)
            {
                return Forbid();
            }
            _organizations.Update(organization);
            return NoContent();
        }

        // POST: api/Organizations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public ActionResult<Organization> PostOrganization(Organization organization)
        {
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Create).Result.Succeeded)
            {
                return Forbid();
            }
            try
            {
                _organizations.Add(organization);
            }
            catch (DbUpdateException)
            {
                if (OrganizationExists(organization.Id))
                {
                    return Conflict();
                }
                throw;
            }
            return CreatedAtAction("GetOrganization", new { id = organization.Id }, organization);
        }

        // DELETE: api/Organizations/5
        [HttpDelete("{id}")]
        public ActionResult<Organization> DeleteOrganization(Guid id)
        {
            var organization = _organizations.Get(id);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Delete).Result.Succeeded)
            {
                return Forbid();
            }
            organization = _organizations.Remove(id);
            if (organization == null)
            {
                return NotFound();
            }
            return organization;
        }

        private bool OrganizationExists(Guid id)
        {
            return _organizations.Get(id) != null;
        }
    }
}
