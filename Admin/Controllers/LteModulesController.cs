using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// GET: api/lte-modules   LTEモジュール一覧照会
    /// </summary>
    [Route("api/lte-modules")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LteModulesController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public LteModulesController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// LTEモジュール一覧照会
        /// GET: api/lte-modules
        /// ロールはSuperAdmin or UserAdminが要る
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<PaginatedResponse<LteModule>> GetLte([FromQuery] GetLteParam param)
        {
            // filter
            var query = _context.LteModule.Where(a => true);
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
            if (param.UseSoftwareRadioState != null) query = query.Where(a => a.UseSoftwareRadioState == param.UseSoftwareRadioState);
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.Name) query = Utils.OrderBy(query, a => a.Name, param.OrderBy).ThenBy(a => a.Name);
            if (param.SortBy == SortKey.UseSoftwareRadioState) query = Utils.OrderBy(query, a => a.UseSoftwareRadioState, param.OrderBy).ThenBy(a => a.Name);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<LteModule> { Count = count, Results = query.ToList() };
        }

        public enum SortKey
        {
            Name,
            UseSoftwareRadioState,
        }

        public class GetLteParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Name;
            public Order OrderBy { get; set; } = Order.Asc;

            public string Name { get; set; }
            public bool? UseSoftwareRadioState { get; set; }
        }
    }
}
