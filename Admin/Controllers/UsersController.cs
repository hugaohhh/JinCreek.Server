using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// GET: api/users/ours      自分ユーザー一覧照会
    /// GET: api/users           ユーザー一覧照会
    /// GET: api/users/ours/{id} 自分ユーザー照会
    /// GET: api/users/{id}      ユーザー照会
    /// PUT: api/users/ours/{id} 自分ユーザー更新（クライアント画面ロック時制御）
    /// PUT: api/users/{id}      ユーザー更新（クライアント画面ロック時制御）
    /// </summary>
    [Route("api/users")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public UsersController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分ユーザー一覧照会
        /// GET: api/users/ours
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("ours")]
        public ActionResult<PaginatedResponse<UserDto>> GetUsers([FromQuery] GetUsersParam param)
        {
            var user = (EndUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var domain = _userRepository.GetDomain(user.Domain.Id);
            return GetUsers(new GetUsersAdminParam(param) { OrganizationCode = domain.Organization.Code });
        }

        /// <summary>
        /// ユーザー一覧照会
        /// GET: api/users
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<UserDto>> GetUsers([FromQuery] GetUsersAdminParam param)
        {
            var currentAvailablePeriod = _context.AvailablePeriod
                    .Join(
                        (
                            _context.AvailablePeriod
                                .GroupBy(ap => new { ap.EndUserId })
                                .Select(r => new
                                {
                                    r.Key.EndUserId,
                                    MaxStartDate = r.Max(ap => ap.StartDate)
                                })
                        ),
                        ap => new { ap.EndUserId, ap.StartDate.Year, ap.StartDate.Month, ap.StartDate.Day, ap.StartDate.Hour, ap.StartDate.Minute, ap.StartDate.Second, ap.StartDate.Millisecond },
                        maxAp => new { maxAp.EndUserId, maxAp.MaxStartDate.Year, maxAp.MaxStartDate.Month, maxAp.MaxStartDate.Day, maxAp.MaxStartDate.Hour, maxAp.MaxStartDate.Minute, maxAp.MaxStartDate.Second, maxAp.MaxStartDate.Millisecond },
                        (ap, max) => new
                        {
                            ap.Id,
                            ap.EndUserId,
                            StartDate = ap.StartDate,
                            EndDate = ap.EndDate
                        }
                    );

            // filter
            var query = _context.EndUser
                .Include(a => a.Domain)
                .Include(a => a.UserGroupEndUsers).ThenInclude(a => a.UserGroup)
                .Include(a => a.AvailablePeriods)
                .Join(
                    currentAvailablePeriod,
                    eu => eu.Id,
                    ap => ap.EndUserId,
                    (endUser, availablePeriod) => new
                    {
                        endUser,
                        availablePeriod
                    }
                )
                .Where(a => a.endUser.Domain.Organization.Code == param.OrganizationCode);
            if (param.DomainId != null) query = query.Where(a => a.endUser.Domain.Id == param.DomainId);
            if (param.UserGroupId != null)
            {
                query = query
                        .Join(
                            _context.UserGroupEndUser,
                            eu => eu.endUser.Id,
                            ugeu => ugeu.EndUser.Id,
                            (eu, ugeu) => new
                            {
                                eu,
                                ugeu.UserGroup.Id
                            }
                        )
                        .Where(a => a.Id == param.UserGroupId)
                        .Select(a => a.eu);
            }
            if (param.AccountName != null) query = query.Where(a => a.endUser.AccountName.Contains(param.AccountName)); // 部分一致
            if (param.Name != null) query = query.Where(a => a.endUser.Name.Contains(param.Name)); // 部分一致
            if (param.StartDateFrom != null) query = query.Where(a => a.availablePeriod.StartDate >= param.StartDateFrom);
            if (param.StartDateTo != null) query = query.Where(a => a.availablePeriod.StartDate <= param.StartDateTo);
            if (param.IncludingEndDateNotSet)
            {
                if (param.EndDateFrom == null && param.EndDateTo == null) query = query.Where(a => a.availablePeriod.EndDate == null);
                if (param.EndDateFrom != null && param.EndDateTo == null) query = query.Where(a => a.availablePeriod.EndDate == null || param.EndDateFrom <= a.availablePeriod.EndDate);
                if (param.EndDateFrom == null && param.EndDateTo != null) query = query.Where(a => a.availablePeriod.EndDate == null || a.availablePeriod.EndDate <= param.EndDateTo);
                if (param.EndDateFrom != null && param.EndDateTo != null) query = query.Where(a => a.availablePeriod.EndDate == null || (param.EndDateFrom <= a.availablePeriod.EndDate && a.availablePeriod.EndDate <= param.EndDateTo));
            }
            else
            {
                if (param.EndDateFrom != null) query = query.Where(a => a.availablePeriod.EndDate >= param.EndDateFrom);
                if (param.EndDateTo != null) query = query.Where(a => a.availablePeriod.EndDate <= param.EndDateTo);
            }
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.DomainName) query = Utils.OrderBy(query, a => a.endUser.Domain.Name, param.OrderBy).ThenBy(a => a.endUser.AccountName);
            if (param.SortBy == SortKey.AccountName) query = Utils.OrderBy(query, a => a.endUser.AccountName, param.OrderBy).ThenBy(a => a.endUser.AccountName);
            if (param.SortBy == SortKey.Name) query = Utils.OrderBy(query, a => a.endUser.Name, param.OrderBy).ThenBy(a => a.endUser.AccountName);
            if (param.SortBy == SortKey.StartDate) query = Utils.OrderBy(query, a => a.availablePeriod.StartDate, param.OrderBy).ThenBy(a => a.endUser.AccountName);
            if (param.SortBy == SortKey.EndDate) query = Utils.OrderBy(query, a => a.availablePeriod.EndDate, param.OrderBy).ThenBy(a => a.endUser.AccountName);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<UserDto>
            {
                Count = count,
                Results = query.Select(a => new UserDto(a.endUser, a.availablePeriod.StartDate, a.availablePeriod.EndDate))
            };
        }

        /// <summary>
        /// 自分ユーザー照会
        /// GET: api/users/ours/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("ours/{id}")]
        public ActionResult<UserDto> GetUser2(Guid id)
        {
            var user = _userRepository.GetEndUser(id);
            var loginUser = (EndUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            if (user.Domain.Organization.Code != loginUser.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return new UserDto(user);
        }

        /// <summary>
        /// ユーザー照会
        /// GET: api/users/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUser(Guid id)
        {
            return new UserDto(_userRepository.GetUser(id));
        }

        /// <summary>
        /// 自分ユーザー更新
        /// PUT: api/users/ours/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("ours/{id}")]
        public ActionResult<UserDto> PutUser(Guid id, PutUserParam param)
        {
            var targetUser = (EndUser)_userRepository.GetUser(id);
            var targetDomain = _userRepository.GetDomain(targetUser.Domain.Id);

            var loginUser = (EndUser)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            var loginDomain = _userRepository.GetDomain(loginUser.Domain.Id);
            if (targetDomain.Organization.Code != loginDomain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }

            targetUser.AuthenticateWhenUnlockingScreen = (bool)param.AuthenticateWhenUnlockingScreen;
            _userRepository.Update(targetUser);
            return Ok(new UserDto(targetUser));
        }

        /// <summary>
        /// ユーザー更新
        /// PUT: api/users/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public ActionResult<UserDto> PutUser2(Guid id, PutUserParam param)
        {
            var user = (EndUser)_userRepository.GetUser(id);
            user.AuthenticateWhenUnlockingScreen = (bool)param.AuthenticateWhenUnlockingScreen;
            _userRepository.Update(user);
            return Ok(new UserDto(user));
        }


        public enum SortKey
        {
            DomainName,
            AccountName,
            Name,
            StartDate,
            EndDate,
        }

        public class GetUsersParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.DomainName;
            public Order OrderBy { get; set; } = Order.Asc;

            public Guid? DomainId { get; set; }
            public Guid? UserGroupId { get; set; }
            public string AccountName { get; set; }
            public string Name { get; set; }
            public DateTime? StartDateFrom { get; set; }
            public DateTime? StartDateTo { get; set; }
            public DateTime? EndDateFrom { get; set; }
            public DateTime? EndDateTo { get; set; }
            public bool IncludingEndDateNotSet { get; set; } = false;
        }

        public class GetUsersAdminParam : GetUsersParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetUsersAdminParam()
            {
            }

            public GetUsersAdminParam(GetUsersParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }

        public class PutUserParam
        {
            /// <summary>
            /// クライアント画面ロック時制御
            /// </summary>
            [Required] public bool? AuthenticateWhenUnlockingScreen { get; set; }
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string AccountName { get; set; }
            public string Name { get; set; }
            public Guid? DomainId { get; set; }
            public bool? AuthenticateWhenUnlockingScreen { get; set; }
            public Domain Domain { get; set; }
            public IEnumerable<UserGroup> UserGroups { get; set; }

            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? StartDate { get; set; }

            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? EndDate { get; set; }

            public UserDto(User user)
            {
                Id = user.Id;
                AccountName = user.AccountName;
                Name = user.Name;
                DomainId = (user as EndUser)?.Domain.Id;
                AuthenticateWhenUnlockingScreen = (user as EndUser)?.AuthenticateWhenUnlockingScreen;
                StartDate = (user as EndUser)?.AvailablePeriods?.OrderByDescending(a => a.StartDate).FirstOrDefault()?.StartDate;
                EndDate = (user as EndUser)?.AvailablePeriods?.OrderByDescending(a => a.StartDate).FirstOrDefault()?.EndDate;
                Domain = (user as EndUser)?.Domain;
                UserGroups = (user as EndUser)?.UserGroupEndUsers?.Select(a => new UserGroup
                { Id = a.UserGroup.Id, Domain = a.UserGroup.Domain, Name = a.UserGroup.Name });
            }

            public UserDto(User user, DateTime? startDate, DateTime? endDate) : this(user)
            {
                StartDate = startDate;
                EndDate = endDate;
            }
        }
    }
}
