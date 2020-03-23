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
    /// GET: api/device-groups/mine 自分端末グループ一覧照会
    /// GET: api/device-groups      端末グループ一覧照会
    /// </summary>
    [Route("api/device-groups")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DeviceGroupsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DeviceGroupsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分端末グループ一覧照会
        /// GET: api/device-groups/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<DeviceGroup>> GetDeviceGroups([FromQuery] GetDeviceGroupsParam param)
        {
            var user = (UserAdmin)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.Domain.Id);
            return GetDeviceGroups(new GetDeviceGroupsAdminParam(param) { OrganizationCode = domain.Organization.Code });
        }

        /// <summary>
        /// 端末グループ一覧照会
        /// GET: api/device-groups?organization=5
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<DeviceGroup>> GetDeviceGroups([FromQuery] GetDeviceGroupsAdminParam param)
        {
            // filter
            var query = _context.DeviceGroup
                .Include(a => a.Domain)
                .Where(a => a.Domain.Organization.Code == param.OrganizationCode);
            if (param.DomainId != null) query = query.Where(a => a.Domain.Id == param.DomainId);
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.DomainName) query = Utils.OrderBy(query, a => a.Domain.Name, param.OrderBy).ThenBy(a => a.Name);
            if (param.SortBy == SortKey.Name) query = Utils.OrderBy(query, a => a.Name, param.OrderBy);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<DeviceGroup> { Count = count, Results = query.ToList() };
        }


        public enum SortKey
        {
            DomainName,
            Name,
        }

        public class GetDeviceGroupsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.DomainName;
            public Order OrderBy { get; set; } = Order.Asc;

            public Guid? DomainId { get; set; }
            public string Name { get; set; }
        }

        public class GetDeviceGroupsAdminParam : GetDeviceGroupsParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetDeviceGroupsAdminParam()
            {
            }

            public GetDeviceGroupsAdminParam(GetDeviceGroupsParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
