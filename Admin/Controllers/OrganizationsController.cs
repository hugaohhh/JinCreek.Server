using Admin.Models;
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
        private readonly IOrganizationRepository _organizations;

        public OrganizationsController(IOrganizationRepository organizations)
        {
            _organizations = organizations;
        }

        // GET: api/Organizations
        [HttpGet]
        public IEnumerable<Organization> GetOrganizations()
        {
            return _organizations.GetAll();
        }

        // GET: api/Organizations/5
        [HttpGet("{id}")]
        public ActionResult<Organization> GetOrganization(string id)
        {
            var organization = _organizations.Get(id);

            if (organization == null)
            {
                return NotFound();
            }

            return organization;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public IActionResult PutOrganization(string id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }

            try
            {
                _organizations.Update(organization);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Organizations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public ActionResult<Organization> PostOrganization(Organization organization)
        {
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
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrganization", new { id = organization.Id }, organization);
        }

        // DELETE: api/Organizations/5
        [HttpDelete("{id}")]
        public ActionResult<Organization> DeleteOrganization(string id)
        {
            var organization = _organizations.Remove(id);
            if (organization == null)
            {
                return NotFound();
            }
            return organization;
        }

        private bool OrganizationExists(string id)
        {
            return _organizations.Get(id) != null;
        }
    }
}
