using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// トップ画面の情報
    /// GET: api/dashboard
    /// GET: api/dashboard/mine
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DashboardController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DashboardController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// ログインユーザの端末件数等を返す
        /// </summary>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<DashboardDto> GetMine()
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            var onlineUsers = _context.MultiFactorAuthenticated
                .Include(m => m.MultiFactor)
                .Include(m => m.MultiFactor.EndUser)
                .Include(m => m.MultiFactor.EndUser.Domain)
                .Count(m => m.MultiFactor.EndUser.Domain.OrganizationCode == user.Domain.OrganizationCode);
            var totalUsers = 0;
            foreach (var endUser in _context.EndUser.Include(e => e.AvailablePeriods)
                .Include(e => e.Domain)
                .Where(e => e.Domain.OrganizationCode == user.Domain.OrganizationCode))
            {
                var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                if (availablePeriod == null) continue;
                if (availablePeriod.StartDate <= DateTime.Now && availablePeriod.EndDate >= DateTime.Now) totalUsers++;
            }
            var totalDevices = _context.Device
                .Include(d => d.Domain)
                .Count(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now && d.Domain.OrganizationCode == user.Domain.OrganizationCode);
            return new DashboardDto { OnlineUsers = onlineUsers, TotalUsers = totalUsers, TotalDevices = totalDevices, WindowsDevices = totalDevices, IosDevices = 0, AndroidDevices = 0, LinuxDevices = 0 };
        }

        /// <summary>
        /// システム全体の端末件数等を返す
        /// </summary>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<DashboardDto> GetAll()
        {
            var onlineUsers = _context.MultiFactorAuthenticated.Count();
            var totalUsers = 0;
            foreach (var endUser in _context.EndUser.Include(e => e.AvailablePeriods))
            {
                var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                if (availablePeriod == null) continue;
                if (availablePeriod.StartDate <= DateTime.Now && availablePeriod.EndDate >= DateTime.Now) totalUsers++;
            }
            var totalDevices = _context.Device.Count(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now);
            return new DashboardDto { OnlineUsers = onlineUsers, TotalUsers = totalUsers, TotalDevices = totalDevices, WindowsDevices = totalDevices, IosDevices = 0, AndroidDevices = 0, LinuxDevices = 0 };
        }

        public class DashboardDto
        {
            public int OnlineUsers { get; set; }
            public int TotalUsers { get; set; }
            public int TotalDevices { get; set; }
            public int WindowsDevices { get; set; }
            public int IosDevices { get; set; }
            public int AndroidDevices { get; set; }
            public int LinuxDevices { get; set; }
        }
    }
}
