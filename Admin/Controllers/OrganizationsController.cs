﻿using Admin.CustomProvider;
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
using System.Security.Claims;

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
        [Authorize(Roles = Roles.SuperAdminUser)]
        public IEnumerable<Organization> GetOrganizations([FromQuery] GetOrganizationsParam param)
        {
            var query = _context.Organization.Where(a => true);
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
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
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("{code}")]
        public ActionResult<Organization> GetOrganization(long code)
        {
            var organization = _userRepository.GetOrganization(code);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Read).Result.Succeeded)
            {
                return new ObjectResult(new { traceId = Activity.Current.Id, errors = new { role = "invalid role" } }) { StatusCode = StatusCodes.Status403Forbidden };
            }
            return organization;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// 組織更新
        /// </summary>
        /// <param name="code"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPut("{code}")]
        public IActionResult PutOrganization(long code, PutOrganizationParam param)
        {
            if (param.GetType().GetProperties().All(a => a.GetValue(param) == null))
            {
                return BadRequest(new { traceId = Activity.Current.Id, errors = new
                {
                    Url = "required",
                    Name = "required",
                    Address = "required",
                    AdminMail = "required",
                    AdminPhone = "required",
                    DelegatePhone = "required",
                    StartDay = "required",
                    EndDay = "required",
                    IsValid = "required",
                }
                });
            }

            var role = User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Role)?.Value;
            if (role == Roles.AdminUser && (param.StartDay != null || param.EndDay != null || param.IsValid != null))
            {
                // ユーザー管理者が利用開始日、利用終了日、有効を指定するのはエラー
                return new ObjectResult(new { traceId = Activity.Current.Id, errors = new { role = "invalid role" } }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            var organization = _userRepository.GetOrganization(code);
            if (organization == null)
            {
                return NotFound(new { traceId = Activity.Current.Id, errors = new { Organization = "not found" } });
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Update).Result.Succeeded)
            {
                return new ObjectResult(new { traceId = Activity.Current.Id, errors = new { role = "invalid role" } }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            organization.Code = param.Code ?? organization.Code;
            organization.Name = param.Name ?? organization.Name;
            organization.Address = param.Address ?? organization.Address;
            organization.DelegatePhone = param.DelegatePhone ?? organization.DelegatePhone;
            organization.Url = param.Url ?? organization.Url;
            organization.AdminPhone = param.AdminPhone ?? organization.AdminPhone;
            organization.AdminMail = param.AdminMail ?? organization.AdminMail;
            organization.StartDay = param.StartDay ?? organization.StartDay;
            organization.EndDay = param.EndDay ?? organization.EndDay;
            organization.IsValid = param.IsValid ?? organization.IsValid;

            _userRepository.Update(organization);
            return Ok(organization);
        }

        public class PutOrganizationParam
        {
            public long? Code { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            [Phone]
            [StringLength(11, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
            public string DelegatePhone { get; set; }
            [Url]
            public string Url { get; set; }
            [Phone]
            [StringLength(11, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
            public string AdminPhone { get; set; }
            [EmailAddress]
            public string AdminMail { get; set; }
            public DateTime? StartDay { get; set; }
            public DateTime? EndDay { get; set; }
            public bool? IsValid { get; set; }
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
                if (OrganizationExists(organization.Code))
                {
                    return Conflict();
                }
                throw;
            }
            return CreatedAtAction("GetOrganization", new { code = organization.Code }, organization);
        }

        // DELETE: api/Organizations/5
        /// <summary>
        /// 組織削除
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpDelete("{code}")]
        [Authorize(Roles = "SuperAdminUser")]
        public ActionResult<Organization> DeleteOrganization(long code)
        {
            var organization = _userRepository.GetOrganization(code);
            if (organization == null)
            {
                return NotFound();
            }
            if (!_authorizationService.AuthorizeAsync(User, organization, Operations.Delete).Result.Succeeded)
            {
                return Forbid();
            }
            organization = _userRepository.RemoveOrganization(code);
            if (organization == null)
            {
                return NotFound();
            }
            return new ObjectResult(organization) { StatusCode = StatusCodes.Status204NoContent };
        }

        private bool OrganizationExists(long code)
        {
            return _userRepository.GetOrganization(code) != null;
        }

        public enum SortKey
        {
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

            public SortKey SortBy { get; set; } = SortKey.Code;
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
