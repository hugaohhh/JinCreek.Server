using Admin.Services;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public OrganizationsController(IAuthorizationService authorizationService, UserRepository userRepository, MainDbContext context)
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _context = context;
        }

        // GET: api/Organizations
        /// <summary>
        /// 組織一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "SuperAdminUser")] // TODO: extract constant
        public IEnumerable<Organization> GetOrganizations([FromQuery] GetOrganizationsParam param)
        {
            var query = _context.Organization.Where(a => true);
            if (param.Name != null) query = query.Where(a => a.Name == param.Name);
            if (param.StartDayFrom != null) query = query.Where(a => a.StartDay >= param.StartDayFrom);
            if (param.StartDayTo != null) query = query.Where(a => a.StartDay <= param.StartDayTo);
            if (param.EndDayFrom != null) query = query.Where(a => a.EndDay >= param.EndDayFrom);
            if (param.EndDayTo != null) query = query.Where(a => a.EndDay <= param.EndDayTo);
            if (param.IsValid != null) query = query.Where(a => a.IsValid == param.IsValid);

            // order by
            if (param.OrderBy == OrderKey.Asc) query = OrderBy(query, param.SortBy.ToString());
            if (param.OrderBy == OrderKey.Desc) query = OrderByDescending(query, param.SortBy.ToString());

            // paging
            return query.Skip((param.Page - 1) * param.PageSize).Take(param.PageSize).ToList();
        }

        // GET: api/Organizations/5
        /// <summary>
        /// 組織照会
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<Organization> GetOrganization(Guid id)
        {
            var organization = _userRepository.GetOrganization(id);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Read).Result.Succeeded)
            {
                return new ObjectResult(new { traceId = Activity.Current.Id }) { StatusCode = StatusCodes.Status403Forbidden };
            }
            return organization;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// 組織更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult PutOrganization(Guid id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }
            if (_userRepository.GetOrganization(id) == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Update).Result.Succeeded)
            {
                return Forbid();
            }
            _userRepository.Update(organization);
            return Ok();
        }

        // POST: api/Organizations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// 組織登録
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "SuperAdminUser")]
        public ActionResult<Organization> PostOrganization(Organization organization)
        {
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Create).Result.Succeeded)
            {
                return Forbid();
            }
            try
            {
                _userRepository.Create(organization);
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
        /// <summary>
        /// 組織削除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdminUser")]
        public ActionResult<Organization> DeleteOrganization(Guid id)
        {
            var organization = _userRepository.GetOrganization(id);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Delete).Result.Succeeded)
            {
                return Forbid();
            }
            organization = _userRepository.Remove(id);
            if (organization == null)
            {
                return NotFound();
            }
            return new ObjectResult(organization) { StatusCode = StatusCodes.Status204NoContent };
        }

        private bool OrganizationExists(Guid id)
        {
            return _userRepository.GetOrganization(id) != null;
        }

        public enum SortKey
        {
            Id,
            Code,
            Name,
            Address,
            DelegatePhone,
            AdminPhone,
            AdminMail,
            StartDay,
            EndDay,
            Url,
            IsValid,
        }

        public enum OrderKey
        {
            Asc,
            Desc,
        }

        public class GetOrganizationsParam
        {
            [Range(1, int.MaxValue)]
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;

            public SortKey SortBy { get; set; } = SortKey.Id;
            public OrderKey OrderBy { get; set; } = OrderKey.Asc;

            public string Name { get; set; }
            public DateTime? StartDayFrom { get; set; }
            public DateTime? StartDayTo { get; set; }
            public DateTime? EndDayFrom { get; set; }
            public DateTime? EndDayTo { get; set; }
            public bool? IsValid { get; set; }
        }

        // see Sorting using property name as string, https://entityframeworkcore.com/knowledge-base/34899933/
        private static IOrderedQueryable<TSource> OrderBy<TSource>(IQueryable<TSource> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");
            Expression property = Expression.Property(parameter, propertyName);
            return (IOrderedQueryable<TSource>)typeof(Queryable).GetMethods()
                .First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), property.Type).Invoke(null,
                    new object[] { source, Expression.Lambda(property, parameter) });
        }

        private static IOrderedQueryable<TSource> OrderByDescending<TSource>(IQueryable<TSource> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TSource), "x");
            Expression property = Expression.Property(parameter, propertyName);
            return (IOrderedQueryable<TSource>)typeof(Queryable).GetMethods()
                .First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), property.Type).Invoke(null,
                    new object[] { source, Expression.Lambda(property, parameter) });
        }
    }
}
