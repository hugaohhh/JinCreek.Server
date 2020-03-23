using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// SIMグループ管理
    /// POST: api/sim-groups                SIMグループ登録
    /// POST: api/sim-groups/mine           自分SIMグループ登録
    /// DELETE: api/sim-groups/{id}         SIMグループ削除
    /// DELETE: api/sim-groups/mine/{id}    自分SIMグループ削除
    /// GET: api/sim-groups                 SIMグループ一覧照会
    /// GET: api/sim-groups/mine            自分SIMグループ一覧照会
    /// GET: api/sim-groups/{id}            SIMグループ照会
    /// GET: api/sim-groups/mine/{id}       自分SIMグループ照会
    /// PUT: api/sim-groups/{id}            SIMグループ更新
    /// PUT: api/sim-groups/mine/{id}       自分SIMグループ更新
    /// </summary>
    [Route("api/sim-groups")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "RedundantCatchClause")]
    public class SimGroupsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public SimGroupsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分SIMグループ登録
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine")]
        public ActionResult<SimGroup> PostSimMine(PostSimGroupParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return PostSim(new PostSimGroupAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        /// <summary>
        /// SIMグループ登録
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult<SimGroup> PostSim(PostSimGroupAdminParam param)
        {
            var organization = _userRepository.GetOrganization((int)param.OrganizationCode);
            var simGroup = new SimGroup()
            {
                Organization = organization,
                Name = param.Name,
                Apn = param.Apn,
                AuthenticationServerIp = param.AuthenticationServerIp,
                NasIp = param.NasIp,
                IsolatedNw1IpPool = param.IsolatedNw1IpPool,
                IsolatedNw1IpRange = param.IsolatedNw1IpRange,
                IsolatedNw1SecondaryDns = param.IsolatedNw1SecondaryDns,
                IsolatedNw1PrimaryDns = param.IsolatedNw1PrimaryDns,
                PrimaryDns = param.PrimaryDns,
                SecondaryDns = param.SecondaryDns,
                UserNameSuffix = param.UserNameSuffix,
            };
            try
            {
                _userRepository.Create(simGroup);
            }
            catch (DbUpdateException)
            {
                if (_context.SimGroup.Any(a => a.Organization.Code == simGroup.Organization.Code && a.Name == simGroup.Name))
                {
                    ModelState.AddModelError(nameof(SimGroup), Messages.Duplicate);
                    return ValidationProblem(ModelState);
                }
                if (_context.SimGroup.Any(a => a.Organization.Code == simGroup.Organization.Code && a.IsolatedNw1IpPool == simGroup.IsolatedNw1IpPool))
                {
                    ModelState.AddModelError(nameof(SimGroup), Messages.Duplicate);
                    return ValidationProblem(ModelState);
                }
                throw;
            }
            return CreatedAtAction("GetSimGroup", new { id = simGroup.Id }, simGroup);
        }

        /// <summary>
        /// 自分SIMグループ削除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpDelete("mine/{id}")]
        public ActionResult<SimGroup> DeleteSimGroupMine(Guid id)
        {
            var simGroup = _userRepository.GetSimGroup(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (user.Domain.Organization.Code != simGroup.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return DeleteSimGroup(id);
        }

        /// <summary>
        /// SIMグループ削除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}")]
        public ActionResult<SimGroup> DeleteSimGroup(Guid id)
        {
            try
            {
                var simGroup = _userRepository.RemoveSimGroup(id);
                return new ObjectResult(simGroup) {StatusCode = StatusCodes.Status204NoContent};
            }
            catch (DbUpdateException)
            {
                if (_context.Sim.Any(a => a.SimGroupId == id))
                {
                    ModelState.AddModelError(nameof(Sim), Messages.ChildEntityExists);
                    return ValidationProblem(ModelState);
                }
                throw;
            }
        }

        /// <summary>
        /// 自分SIMグループ一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<SimGroup>> GetSimGroups([FromQuery] GetSimGroupsParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetSimGroups(new GetSimGroupsAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        /// <summary>
        /// SIMグループ一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<SimGroup>> GetSimGroups([FromQuery] GetSimGroupsAdminParam param)
        {
            // filter
            var query = _context.SimGroup.Where(a => a.Organization.Code == param.OrganizationCode);
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
            if (param.Apn != null) query = query.Where(a => a.Apn.Contains(param.Apn));
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.Name) query = Utils.OrderBy(query, a => a.Name, param.OrderBy).ThenBy(a => a.Apn);
            if (param.SortBy == SortKey.Apn) query = Utils.OrderBy(query, a => a.Apn, param.OrderBy);
            if (param.SortBy == SortKey.NasIp) query = Utils.OrderBy(query, a => a.NasIp, param.OrderBy);
            if (param.SortBy == SortKey.AuthenticationServerIp) query = Utils.OrderBy(query, a => a.AuthenticationServerIp, param.OrderBy);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<SimGroup> { Count = count, Results = query.ToList() };
        }

        /// <summary>
        /// 自分SIMグループ照会
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/{id}")]
        public ActionResult<SimGroup> GetSimGroup(Guid id)
        {
            var simGroup = _userRepository.GetSimGroup(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (simGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return simGroup;
        }

        /// <summary>
        /// SIMグループ照会
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<SimGroup> GetSimGroup2(Guid id)
        {
            return _userRepository.GetSimGroup(id);
        }

        /// <summary>
        /// 自分SIMグループ更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine/{id}")]
        public IActionResult UpdateSimGroupMine(Guid id, PutSimGroupParam param)
        {
            var simGroup = _userRepository.GetSimGroup(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (simGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return UpdateSimGroup(id, param);
        }

        /// <summary>
        /// SIMグループ更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public IActionResult UpdateSimGroup(Guid id, PutSimGroupParam param)
        {
            var simGroup = _userRepository.GetSimGroup(id);
            simGroup.Name = param.Name;
            simGroup.Apn = param.Apn;
            simGroup.IsolatedNw1IpPool = param.IsolatedNw1IpPool;
            simGroup.IsolatedNw1IpRange = param.IsolatedNw1IpRange;
            simGroup.IsolatedNw1SecondaryDns = param.IsolatedNw1SecondaryDns;
            simGroup.IsolatedNw1PrimaryDns = param.IsolatedNw1PrimaryDns;
            simGroup.PrimaryDns = param.PrimaryDns;
            simGroup.SecondaryDns = param.SecondaryDns;
            simGroup.AuthenticationServerIp = param.AuthenticationServerIp;
            simGroup.NasIp = param.NasIp;
            simGroup.UserNameSuffix = param.UserNameSuffix;
            try
            {
                _userRepository.Update(simGroup);
                return Ok(_userRepository.GetSimGroup(id));
            }
            catch (DbUpdateException)
            {
                if (_context.SimGroup.Any(a => a.Id != id && a.Organization.Code == simGroup.Organization.Code && a.Name == simGroup.Name))
                {
                    ModelState.AddModelError(nameof(SimGroup), Messages.Duplicate);
                    return ValidationProblem(ModelState);
                }
                if (_context.SimGroup.Any(a => a.Id != id && a.Organization.Code == simGroup.Organization.Code && a.IsolatedNw1IpPool == simGroup.IsolatedNw1IpPool))
                {
                    ModelState.AddModelError(nameof(SimGroup), Messages.Duplicate);
                    return ValidationProblem(ModelState);
                }
                throw;
            }
        }

        public enum SortKey
        {
            Name,
            Apn,
            AuthenticationServerIp,
            NasIp
        }

        public class SimParam
        {
            [Required] public string Name { get; set; }
            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = Messages.ApnIsOnlyAscii)]
            [Required]
            public string Apn { get; set; }

            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = Messages.IsolatedNw1IpPoolIsOnlyAscii)]
            [Required]
            public string IsolatedNw1IpPool { get; set; }

            [RegularExpression("^(?:(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\/([1-9]|[1-2]\\d|3[0-2])$", ErrorMessage = Messages.InvalidCIDR)]
            [Required]
            public string IsolatedNw1IpRange { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            [Required]
            public string AuthenticationServerIp { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            //[Required]
            public string IsolatedNw1SecondaryDns { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            //[Required]
            public string IsolatedNw1PrimaryDns { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            [Required]
            public string NasIp { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            [Required]
            public string PrimaryDns { get; set; }

            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}", ErrorMessage = Messages.InvalidIpAddress)]
            [Required]
            public string SecondaryDns { get; set; }

            [Required] public string UserNameSuffix { get; set; }
        }

        public class PutSimGroupParam : SimParam
        {
        }

        public class PostSimGroupParam : SimParam
        {
        }

        public class PostSimGroupAdminParam : PostSimGroupParam
        {
            [Required] public int? OrganizationCode { get; set; }
            public PostSimGroupAdminParam()
            {
            }

            public PostSimGroupAdminParam(PostSimGroupParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }

        public class GetSimGroupsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Name;
            public Order OrderBy { get; set; } = Order.Asc;

            public string Name { get; set; }
            public string Apn { get; set; }
        }

        public class GetSimGroupsAdminParam : GetSimGroupsParam
        {
            [Required] public int? OrganizationCode { get; set; }

            public GetSimGroupsAdminParam()
            {
            }

            public GetSimGroupsAdminParam(GetSimGroupsParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
