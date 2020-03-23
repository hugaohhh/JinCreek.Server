using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// GET: api/domains/mine      自分ドメイン一覧照会
    /// GET: api/domains           ドメイン一覧照会
    /// </summary>
    [Route("api/domains")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DomainsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DomainsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分ドメイン一覧照会
        /// GET: api/domains/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<Domain>> GetDomains([FromQuery] GetDomainsParam param)
        {
            var user = (UserAdmin)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.Domain.Id);
            return GetDomains(new GetDomainsAdminParam(param) { OrganizationCode = domain.Organization.Code });
        }

        /// <summary>
        /// ドメイン一覧照会
        /// GET: api/domains?organization=5
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<Domain>> GetDomains([FromQuery] GetDomainsAdminParam param)
        {
            // filter
            var query = _context.Domain.Where(a => a.Organization.Code == param.OrganizationCode);
            var count = query.Count();

            // ordering
            query = Utils.OrderBy(query, param.SortBy.ToString(), param.OrderBy);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<Domain> { Count = count, Results = query.ToList() };
        }


        public enum SortKey
        {
            Id,
            Name,
        }

        public class GetDomainsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Name;
            public Order OrderBy { get; set; } = Order.Asc;
        }

        public class GetDomainsAdminParam : GetDomainsParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetDomainsAdminParam()
            {
            }

            public GetDomainsAdminParam(GetDomainsParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
