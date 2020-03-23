using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// 組織端末アプリ照会
    /// 実験用
    /// </summary>
    [Route("api/organization-client-apps")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class OrganizationClientAppsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public OrganizationClientAppsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<OrganizationClientApp>> Get([FromQuery] GetOrganizationClientAppParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return Get(new GetOrganizationClientAppAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<OrganizationClientApp>> Get([FromQuery] GetOrganizationClientAppAdminParam param)
        {
            // filter
            var query = _context.OrganizationClientApp
                .Include(a => a.ClientApp.ClientOs)
                .Where(a => a.Organization.Code == param.OrganizationCode);
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.ClientOsName) query = Utils.OrderBy(query, a => a.ClientApp.ClientOs.Name, param.OrderBy).ThenBy(a => a.ClientApp.Version);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<OrganizationClientApp> { Count = count, Results = query.ToList() };
        }

        public enum SortKey
        {
            ClientOsName,
        }

        public class GetOrganizationClientAppParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.ClientOsName;
            public Order OrderBy { get; set; } = Order.Asc;
        }

        public class GetOrganizationClientAppAdminParam : GetOrganizationClientAppParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetOrganizationClientAppAdminParam()
            {
            }

            public GetOrganizationClientAppAdminParam(GetOrganizationClientAppParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
