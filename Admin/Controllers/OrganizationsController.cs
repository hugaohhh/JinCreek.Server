using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// POST: api/organizations          組織登録
    /// GET: api/organizations           組織一覧照会
    /// GET: api/organizations/mine      自己組織照会
    /// GET: api/organizations/{code}    組織照会
    /// PUT: api/organizations/mine      自己組織更新
    /// PUT: api/organizations/{code}    組織更新
    /// DELETE: api/organizations/{code} 組織削除
    /// </summary>
    [Route("api/organizations")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class OrganizationsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public OrganizationsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 組織一覧照会
        /// GET: api/organizations
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin)]
        public ActionResult<PaginatedResponse<Organization>> GetOrganizations([FromQuery] GetOrganizationsParam param)
        {
            // filter
            var query = _context.Organization.Where(a => true);
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
            if (param.StartDateFrom != null) query = query.Where(a => a.StartDate >= param.StartDateFrom);
            if (param.StartDateTo != null) query = query.Where(a => a.StartDate <= param.StartDateTo);
            if (param.IncludingEndDateNotSet)
            {
                if (param.EndDateFrom == null && param.EndDateTo == null) query = query.Where(a => a.EndDate == null);
                if (param.EndDateFrom != null && param.EndDateTo == null) query = query.Where(a => a.EndDate == null || param.EndDateFrom <= a.EndDate);
                if (param.EndDateFrom == null && param.EndDateTo != null) query = query.Where(a => a.EndDate == null || a.EndDate <= param.EndDateTo);
                if (param.EndDateFrom != null && param.EndDateTo != null) query = query.Where(a => a.EndDate == null || (param.EndDateFrom <= a.EndDate && a.EndDate <= param.EndDateTo));
            }
            else
            {
                if (param.EndDateFrom != null) query = query.Where(a => a.EndDate >= param.EndDateFrom);
                if (param.EndDateTo != null) query = query.Where(a => a.EndDate <= param.EndDateTo);
            }
            if (param.IsValid != null) query = query.Where(a => a.IsValid == param.IsValid);
            var count = query.Count();

            // sort
            query = Utils.OrderBy(query, param.SortBy.ToString(), param.OrderBy).ThenBy(a => a.Code);

            // pagination
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<Organization> { Count = count, Results = query.ToList() };
        }

        /// <summary>
        /// 自己組織照会
        /// GET: api/organizations/mine
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<Organization> GetOrganization()
        {
            var user = (EndUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.Domain.Id);
            return GetOrganization(domain.Organization.Code);
        }

        /// <summary>
        /// 組織照会
        /// GET: api/organizations/5
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{code}")]
        public ActionResult<Organization> GetOrganization(int code)
        {
            return _userRepository.GetOrganization(code);
        }

        /// <summary>
        /// 自己組織更新
        /// PUT: api/organizations/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine")]
        public IActionResult PutOrganization(PutOrganizationParam param)
        {
            var user = (EndUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.Domain.Id);
            var organization = _userRepository.GetOrganization(domain.Organization.Code);
            organization.Name = param.Name;
            organization.Address = param.Address;
            organization.Phone = param.Phone;
            organization.Url = param.Url;
            organization.AdminPhone = param.AdminPhone;
            organization.AdminMail = param.AdminMail;

            try
            {
                _userRepository.Update(organization);
                return Ok(organization);
            }
            catch (DbUpdateException ex)
            {
                if (_context.Organization.Any(a => a.Name == organization.Name))
                {
                    ModelState.AddModelError(nameof(organization.Name), ex.InnerException?.Message);
                    return ValidationProblem(ModelState);
                }
                throw;
            }
        }

        /// <summary>
        /// 組織更新
        /// PUT: api/organizations/5
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see https://aka.ms/RazorPagesCRUD.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{code}")]
        public IActionResult PutOrganization(int code, PutOrganizationAdminParam param)
        {
            var organization = _userRepository.GetOrganization(code);
            organization.Name = param.Name;
            organization.Address = param.Address;
            organization.Phone = param.Phone;
            organization.Url = param.Url;
            organization.AdminPhone = param.AdminPhone;
            organization.AdminMail = param.AdminMail;
            organization.StartDate = param.StartDate;
            organization.EndDate = param.EndDate;
            organization.IsValid = param.IsValid;
            organization.DistributionServerIp = param.DistributionServerIp;
            try
            {
                _userRepository.Update(organization);
                return Ok(organization);
            }
            catch (DbUpdateException ex)
            {
                if (_context.Organization.Any(a => a.Name == organization.Name))
                {
                    ModelState.AddModelError(nameof(organization.Name), ex.InnerException?.Message);
                    return ValidationProblem(ModelState);
                }
                throw;
            }
        }

        /// <summary>
        /// 組織登録
        /// POST: api/organizations
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see https://aka.ms/RazorPagesCRUD.
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult<Organization> PostOrganization(Organization organization)
        {
            try
            {
                _userRepository.Create(organization);
                return CreatedAtAction("GetOrganization", new { code = organization.Code }, organization);
            }
            catch (DbUpdateException ex)
            {
                if (_context.Organization.Any(a => a.Code == organization.Code))
                {
                    ModelState.AddModelError(nameof(organization.Code), ex.InnerException?.Message);
                }
                if (_context.Organization.Any(a => a.Name == organization.Name))
                {
                    ModelState.AddModelError(nameof(organization.Name), ex.InnerException?.Message);
                }
                if (!ModelState.IsValid) return ValidationProblem(ModelState);
                throw;
            }
        }

        /// <summary>
        /// 組織削除
        /// DELETE: api/organizations/5
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{code}")]
        public ActionResult<Organization> DeleteOrganization(int code)
        {
            _userRepository.GetOrganization(code);

            try
            {
                var organization = _userRepository.RemoveOrganization(code);
                return new ObjectResult(organization) { StatusCode = StatusCodes.Status204NoContent };
            }
            catch (DbUpdateException)
            {
                var org = _context.Organization.AsNoTracking().FirstOrDefault(a => a.Code == code);
                if (_context.Domain.Any(a => a.Organization == org))
                {
                    ModelState.AddModelError(nameof(Domain), Messages.ChildEntityExists);
                }
                if (_context.SimGroup.Any(a => a.Organization == org))
                {
                    ModelState.AddModelError(nameof(SimGroup), Messages.ChildEntityExists);
                }
                if (_context.OrganizationClientApp.Any(a => a.Organization == org))
                {
                    ModelState.AddModelError(nameof(OrganizationClientApp), Messages.ChildEntityExists);
                }
                if (!ModelState.IsValid) return ValidationProblem(ModelState);
                throw;
            }
        }

        private bool OrganizationExists(int code)
        {
            return _context.Organization.Any(a => a.Code == code);
        }

        public class PutOrganizationParam
        {
            [Required] public string Name { get; set; }
            [Required] public string Address { get; set; }

            [Phone]
            [StringLength(11, MinimumLength = 10)]
            [Required]
            public string Phone { get; set; }

            [Url] [Required] public string Url { get; set; }

            [Phone]
            [StringLength(11, MinimumLength = 10)]
            [Required]
            public string AdminPhone { get; set; }

            [EmailAddress] [Required] public string AdminMail { get; set; }

        }

        public class PutOrganizationAdminParam : PutOrganizationParam, IValidatableObject
        {
            [Required] public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            [Required] public bool? IsValid { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            [Required]
            public string DistributionServerIp { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (StartDate > EndDate)
                {
                    yield return new ValidationResult(Messages.InvalidEndDate, new[] { nameof(EndDate) });
                }
            }
        }

        public enum SortKey
        {
            Code,
            Name,
            Address,
            Phone,
            AdminPhone,
            AdminMail,
            StartDate,
            EndDate,
            Url,
            IsValid,
        }

        public class GetOrganizationsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Code;
            public Order OrderBy { get; set; } = Order.Asc;

            public string Name { get; set; }
            public DateTime? StartDateFrom { get; set; }
            public DateTime? StartDateTo { get; set; }
            public DateTime? EndDateFrom { get; set; }
            public DateTime? EndDateTo { get; set; }
            public bool IncludingEndDateNotSet { get; set; } = false;
            public bool? IsValid { get; set; }
        }
    }
}
