using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// 認証ログ
    /// GET: api/authentication-logs/mine  自分認証ログ一覧照会
    /// GET: api/authentication-logs       認証ログ一覧照会
    /// </summary>
    [Route("api/authentication-logs")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "PossibleInvalidCastException")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AuthenticationLogsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public AuthenticationLogsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分認証ログ一覧照会
        /// GET: api/authentication-logs/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<AuthenticationLogDto>> GetLogs([FromQuery] GetAuthenticationLogParam param)
        {
            var user = (UserAdmin)_userRepository.GetUser(Guid.Parse(User.Identity.Name));
            return GetLogs(new GetAuthenticationLogAdminParam(param) { OrganizationCode = user.Domain.OrganizationCode });
        }

        /// <summary>
        /// 認証ログ一覧照会
        /// GET: api/authentication-logs?organizationCode=5
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<AuthenticationLogDto>> GetLogs([FromQuery] GetAuthenticationLogAdminParam param)
        {
            // SIM, Device, EndUserまでたどる
            // see https://docs.microsoft.com/ja-jp/ef/core/querying/related-data#include-on-derived-types
            var query = _context.AuthenticationLog
                .Include(a => a.Sim.SimGroup.Organization)
                .Include(a => (a as SimAndDeviceAuthenticationSuccessLog).SimAndDevice.Device)
                .Include(a => (a as SimAndDeviceAuthenticationFailureLog).Sim)
                .Include(a => (a as MultiFactorAuthenticationSuccessLog).MultiFactor.SimAndDevice.Device)
                .Include(a => (a as MultiFactorAuthenticationSuccessLog).MultiFactor.EndUser)
                .Include(a => (a as MultiFactorAuthenticationFailureLog).SimAndDevice.Device)
                .Include(a => (a as DeauthenticationLog).MultiFactor.SimAndDevice.Device)
                .Include(a => (a as DeauthenticationLog).MultiFactor.EndUser)
                .Where(a => a.Sim.SimGroup.OrganizationCode == param.OrganizationCode);
            if (param.TimeFrom != null) query = query.Where(a => a.Time >= param.TimeFrom);
            if (param.TimeTo != null) query = query.Where(a => a.Time <= param.TimeTo);
            if (param.SimMsisdn != null) query = query.Where(a => a.Sim.Msisdn.Contains(param.SimMsisdn));
            if (param.DeviceName != null)
            {
                query = query.Where(a =>
                    ((SimAndDeviceAuthenticationSuccessLog)a).SimAndDevice.Device.Name.Contains(param.DeviceName) ||
                    ((MultiFactorAuthenticationFailureLog)a).SimAndDevice.Device.Name.Contains(param.DeviceName) ||
                    ((DeauthenticationLog)a).MultiFactor.SimAndDevice.Device.Name.Contains(param.DeviceName) ||
                    ((MultiFactorAuthenticationSuccessLog)a).MultiFactor.SimAndDevice.Device.Name.Contains(param.DeviceName)
                );
            }
            if (param.UserAccountName != null)
            {
                query = query.Where(a =>
                    ((DeauthenticationLog)a).MultiFactor.EndUser.AccountName.Contains(param.UserAccountName) ||
                    ((MultiFactorAuthenticationSuccessLog)a).MultiFactor.EndUser.AccountName.Contains(param.UserAccountName)
                );
            }
            if (param.Type == TypeKey.SimAndDeviceAuthenticationSuccessLog) query = query.OfType<SimAndDeviceAuthenticationSuccessLog>();
            if (param.Type == TypeKey.SimAndDeviceAuthenticationFailureLog) query = query.OfType<SimAndDeviceAuthenticationFailureLog>();
            if (param.Type == TypeKey.MultiFactorAuthenticationSuccessLog) query = query.OfType<MultiFactorAuthenticationSuccessLog>();
            if (param.Type == TypeKey.MultiFactorAuthenticationFailureLog) query = query.OfType<MultiFactorAuthenticationFailureLog>();
            if (param.Type == TypeKey.DeauthenticationLog) query = query.OfType<DeauthenticationLog>();
            var count = query.Count();

            // sort
            if (param.SortBy == SortKey.Time) query = Utils.OrderBy(query, a => a.Time, param.OrderBy).ThenBy(a => a.Id);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<AuthenticationLogDto> { Count = count, Results = query.Select(a => new AuthenticationLogDto(a)) };
        }

        public enum SortKey
        {
            Time,
        }

        public enum TypeKey
        {
            SimAndDeviceAuthenticationSuccessLog,
            SimAndDeviceAuthenticationFailureLog,
            MultiFactorAuthenticationSuccessLog,
            MultiFactorAuthenticationFailureLog,
            DeauthenticationLog,
        }

        public class GetAuthenticationLogParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Time;
            public Order OrderBy { get; set; } = Order.Desc;

            public DateTime? TimeFrom { get; set; }
            public DateTime? TimeTo { get; set; }
            public string SimMsisdn { get; set; }
            public string DeviceName { get; set; }
            public string UserAccountName { get; set; }
            public TypeKey? Type { get; set; }
        }

        public class GetAuthenticationLogAdminParam : GetAuthenticationLogParam
        {
            [Required] public int? OrganizationCode { get; set; }

            public GetAuthenticationLogAdminParam()
            {
            }

            public GetAuthenticationLogAdminParam(GetAuthenticationLogParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }

        public class AuthenticationLogDto
        {
            public Guid Id { get; set; }
            public TypeKey Type { get; set; }
            public DateTime Time { get; set; }
            public SimAndDevice SimAndDevice { get; set; }
            public Sim Sim { get; set; }
            public MultiFactor MultiFactor { get; set; }

            public AuthenticationLogDto(AuthenticationLog log)
            {
                Id = log.Id;
                Type = (TypeKey)Enum.Parse(typeof(TypeKey), log.GetType().ShortDisplayName());
                Time = log.Time;
                Sim = log.Sim;
                SimAndDevice = log switch
                {
                    SimAndDeviceAuthenticationSuccessLog a => a.SimAndDevice,
                    SimAndDeviceAuthenticationFailureLog _ => null,
                    MultiFactorAuthenticationSuccessLog _ => null,
                    MultiFactorAuthenticationFailureLog a => a.SimAndDevice,
                    DeauthenticationLog _ => null,
                    _ => null
                };
                MultiFactor = log switch
                {
                    SimAndDeviceAuthenticationSuccessLog _ => null,
                    SimAndDeviceAuthenticationFailureLog _ => null,
                    MultiFactorAuthenticationSuccessLog a => a.MultiFactor,
                    MultiFactorAuthenticationFailureLog _ => null,
                    DeauthenticationLog a => a.MultiFactor,
                    _ => null
                };
            }
        }
    }
}
