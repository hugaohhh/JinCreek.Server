using CsvHelper;
using CsvHelper.Configuration;
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
using System.Globalization;
using System.IO;
using System.Linq;

namespace JinCreek.Server.Admin.Controllers
{
    /// <summary>
    /// SIM＆端末組合わせ
    /// POST api/sim-and-devices                SIM＆端末登録
    /// POST api/sim-and-devices/mine           自分SIM＆端末登録
    /// GET: api/sim-and-devices                SIM＆端末組合わせ一覧照会
    /// GET: api/sim-and-devices/mine           自分SIM＆端末組合わせ一覧照会
    /// GET: api/sim-and-devices/{id}           SIM＆端末組合わせ照会
    /// GET: api/sim-and-devices/mine/{id}      自分SIM＆端末組合わせ照会
    /// DELETE api/sim-and-devices/{id}         SIM＆端末削除
    /// DELETE api/sim-and-devices/mine/{id}    自分SIM＆端末削除
    /// PUT api/sim-and-devices/{id}            SIM＆端末更新
    /// PUT api/sim-and-devices/mine/{id}       自分SIM＆端末更新
    /// GET api/sim-and-devices/csv             SIM＆端末エクスポート
    /// GET api/sim-and-devices/mine/csv        自分SIM＆端末エクスポート
    /// POST api/sim-and-devices/csv            SIM＆端末インポート
    /// POST api/sim-and-devices/mine/csv       自分SIM＆端末インポート
    /// </summary>
    [Route("api/sim-and-devices")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SimAndDevicesController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public SimAndDevicesController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分SimDevice登録
        /// POST api/-devices/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine")]
        public ActionResult<SimAndDevice> PostSimAndDeviceMine(RegisterParam param)
        {
            var sim = _userRepository.GetSim((Guid)param.SimId);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            if (!_context.Device.Any(d => d.Id == param.DeviceId && d.Domain.Organization.Code == user.Domain.Organization.Code)
            && _context.Device.Any(d => d.Id == param.DeviceId))
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PostSimAndDevice(param);
        }

        /// <summary>
        /// SimDevice登録
        /// POST api/-devices/
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult<SimAndDevice> PostSimAndDevice(RegisterParam param)
        {
            var sim = _userRepository.GetSim((Guid)param.SimId);
            var device = _userRepository.GetDevice((Guid)param.DeviceId);

            if (param.StartDate < device.StartDate || param.StartDate > device.EndDate)
                ModelState.AddModelError(nameof(param.StartDate), Messages.OutOfDate);
            if (param.EndDate < device.StartDate || param.EndDate > device.EndDate)
                ModelState.AddModelError(nameof(param.EndDate), Messages.OutOfDate);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var simDevice = new SimAndDevice()
            {
                Sim = sim,
                Device = device,
                StartDate = (DateTime)param.StartDate,
                EndDate = param.EndDate,
                AuthenticationDuration = (int)param.AuthenticationDuration,
                IsolatedNw2Ip = param.IsolatedNw2Ip
            };
            if (_context.SimAndDevice.Any(s => s.Sim.Id == simDevice.Sim.Id && s.Device.Id == simDevice.Device.Id))
            {
                ModelState.AddModelError(nameof(SimAndDevice), Messages.Duplicate);
                return ValidationProblem(ModelState);
            }
            try
            {
                _userRepository.Create(simDevice);
            }
            catch (DbUpdateException)
            {
                return Conflict();
                //throw;
            }
            return CreatedAtAction("GetSimAndDevice", new { id = simDevice.Id }, simDevice);
        }

        /// <summary>
        /// 自分SIMDevice削除
        /// DELETE api/sim-and-devices/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpDelete("mine/{id}")]
        public ActionResult<SimAndDevice> DeleteSimAndDeviceMine(Guid id)
        {
            var simDevice = _userRepository.GetSimAndDevice(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (simDevice.Sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return DeleteSimAndDevice(id);
        }

        /// <summary>
        /// SIMDevice削除
        /// DELETE api/sim-and-devices/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}")]
        public ActionResult<SimAndDevice> DeleteSimAndDevice(Guid id)
        {
            if (_context.MultiFactor.Any(f => f.SimAndDeviceId == id))
                ModelState.AddModelError(nameof(MultiFactor), Messages.ChildEntityExists);
            if (_context.MultiFactorAuthenticationFailureLog.Any(f => f.SimAndDevice.Id == id))
                ModelState.AddModelError(nameof(MultiFactorAuthenticationFailureLog), Messages.ChildEntityExists);
            if (_context.SimAndDeviceAuthenticated.Any(s => s.SimAndDeviceId == id))
                ModelState.AddModelError(nameof(SimAndDeviceAuthenticated), Messages.ChildEntityExists);
            if (_context.SimAndDeviceAuthenticationSuccessLog.Any(f => f.SimAndDevice.Id == id))
                ModelState.AddModelError(nameof(SimAndDeviceAuthenticationSuccessLog), Messages.ChildEntityExists);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var simDevice = _userRepository.RemoveSimAndDevice(id);
            return new ObjectResult(simDevice) { StatusCode = StatusCodes.Status204NoContent };
        }

        /// <summary>
        /// 自分SIM＆端末組合わせ一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<SimAndDevice>> GetSimAndDevices([FromQuery] GetSimAndDevicesParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetSimAndDevices(new GetSimAndDevicesAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        /// <summary>
        /// SIM＆端末組合わせ一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<SimAndDevice>> GetSimAndDevices([FromQuery] GetSimAndDevicesAdminParam param)
        {
            // filter
            var query = _context.SimAndDevice.Include(s => s.Sim)
                .Include(s => s.Device).Include(s => s.SimAndDeviceAuthenticated)
                .Where(a => a.Sim.SimGroup.Organization.Code == param.OrganizationCode);
            if (param.Msisdn != null) query = query.Where(a => a.Sim.Msisdn.Contains(param.Msisdn));
            if (param.DeviceName != null) query = query.Where(a => a.Device.Name.Contains(param.DeviceName));
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
            if (param.IsAuthenticationDone != null)
                query = (bool)param.IsAuthenticationDone ? query.Where(a => a.SimAndDeviceAuthenticated != null) : query.Where(a => a.SimAndDeviceAuthenticated == null);
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.Msisdn) query = Utils.OrderBy(query, a => a.Sim.Msisdn, param.OrderBy).ThenBy(a => a.Device.Name);
            if (param.SortBy == SortKey.DeviceName)
                query = Utils.OrderBy(query, a => a.Device.Name, param.OrderBy);
            if (param.SortBy == SortKey.IsolatedNw2Ip)
                query = Utils.OrderBy(query, a => a.IsolatedNw2Ip, param.OrderBy);
            if (param.SortBy == SortKey.StartDate)
                query = Utils.OrderBy(query, a => a.StartDate, param.OrderBy);
            if (param.SortBy == SortKey.EndDate)
                query = Utils.OrderBy(query, a => a.EndDate, param.OrderBy);
            if (param.SortBy == SortKey.AuthenticationDuration)
                query = Utils.OrderBy(query, a => a.AuthenticationDuration, param.OrderBy);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<SimAndDevice> { Count = count, Results = query.ToList() };
        }

        /// <summary>
        /// 自分SimDevice照会（ログインユーザーが所属する組織のSImDevice一覧）
        /// GET: api/sim-and-devices/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/{id}")]
        public ActionResult<SimAndDevice> GetSimAndDevice(Guid id)
        {
            var simAndDevice = _userRepository.GetSimAndDevice(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (simAndDevice.Sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return simAndDevice;
        }

        /// <summary>
        /// SimDevice照会
        /// GET: api/sim-and-devices/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<SimAndDevice> GetSimAndDevice2(Guid id)
        {
            var simAndDevice = _userRepository.GetSimAndDevice(id);
            return simAndDevice;
        }

        /// <summary>
        /// 自分SimDevice更新
        /// PUT: api/sim-and-devices/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine/{id}")]
        public IActionResult PutSimAndDeviceMine(Guid id, PutSimAndDeviceParam param)
        {
            var simDevice = _userRepository.GetSimAndDevice(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (simDevice.Sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PutSimAndDevice(id, param);
        }

        /// <summary>
        /// SimDevice更新
        /// PUT: api/sim-and-devices/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public IActionResult PutSimAndDevice(Guid id, PutSimAndDeviceParam param)
        {
            var simDevice = _userRepository.GetSimAndDevice(id);
            if (param.StartDate < simDevice.Device.StartDate || param.StartDate > simDevice.Device.EndDate)
                ModelState.AddModelError(nameof(param.StartDate), Messages.OutOfDate);
            if (param.EndDate < simDevice.Device.StartDate || param.EndDate > simDevice.Device.EndDate)
                ModelState.AddModelError(nameof(param.EndDate), Messages.OutOfDate);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            simDevice.AuthenticationDuration = (int)param.AuthenticationDuration;
            simDevice.StartDate = (DateTime)param.StartDate;
            simDevice.EndDate = param.EndDate;
            simDevice.IsolatedNw2Ip = param.IsolatedNw2Ip;
            _userRepository.Update(simDevice);
            return Ok(simDevice);
        }

        /// <summary>
        /// 自分SimDeviceエクスポート
        /// GET: api/sim-and-devices/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/csv")]
        public ActionResult<string> GetSimAndDevicesCsv()
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetSimAndDevicesCsv(user.Domain.Organization.Code);
        }

        /// <summary>
        /// SimDeviceエクスポート
        /// GET: api/sim-and-devices/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("csv")]
        public ActionResult<string> GetSimAndDevicesCsv([FromQuery] [Required] long organizationCode)
        {
            var simDevices = _context.SimAndDevice
                .Include(a => a.Sim)
                .Include(a => a.Device)
                .Include(a => a.Device.Domain)
                .Where(a => a.Sim.SimGroup.Organization.Code == organizationCode)
                .OrderBy(a => a.Sim.Msisdn)
                .ThenBy(a => a.Device.Name)
                .ToList();

            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.Configuration.RegisterClassMap<SimAndDeviceMap>();
            csvWriter.WriteRecords(simDevices);
            return writer.ToString();
        }

        /// <summary>
        /// CSVエクスポートのためのマップ
        /// </summary>
        public sealed class SimAndDeviceMap : ClassMap<SimAndDevice>
        {
            public SimAndDeviceMap()
            {
                Map().Name("is Delete").Index(0).ConvertUsing(a => "");
                Map(a => a.Sim.Id).Name("SIM ID").Index(1);
                Map(a => a.Sim.Msisdn).Name("MSISDN").Index(2);
                Map(a => a.Device.Domain.Name).Name("Domain").Index(3);
                Map(a => a.Device.Id).Name("Device ID").Index(4);
                Map(a => a.Device.Name).Name("Device Name").Index(5);
                Map(a => a.IsolatedNw2Ip).Name("Isolated NW2 IP").Index(6);
                Map(a => a.AuthenticationDuration).Name("Authentication Duration").Index(7);
                Map(a => a.StartDate).Name("Start Date").Index(8).TypeConverterOption.Format("yyyy-MM-dd");
                Map(a => a.EndDate).Name("End Date").Index(9).TypeConverterOption.Format("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 自分simDeviceインポート
        /// POST: api/sims/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine/csv")]
        public ActionResult<IEnumerable<SimAndDevice>> PostSimAndDevicesCsv(IFormFile csv)
        {
            using var reader = new StreamReader(csv.OpenReadStream());
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return ImportCsv(reader, user);
        }

        /// <summary>
        /// Simインポート
        /// POST: api/sims/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("csv")]
        public ActionResult<IEnumerable<SimAndDevice>> PostSimAndDevicesCsv2(IFormFile csv)
        {
            using var reader = new StreamReader(csv.OpenReadStream());
            return ImportCsv(reader);
        }

        /// <summary>
        /// CSVインポート
        /// </summary>
        /// <param name="csv"></param>
        /// <param name="endUser"></param>
        /// <returns></returns>
        private ActionResult<IEnumerable<SimAndDevice>> ImportCsv(TextReader csv, EndUser endUser = null)
        {
            // 1. 型変換エラーがあればここで終了する：
            var records = Utils.ParseCsv<CsvRecord, CsvRecordMap>(csv, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // 2. バリデーションエラーがあればここで終了する：
            Utils.TryValidateCsvRecords(records, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var simDevices = new List<SimAndDevice>();
            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var record in records)
                {
                    var sim = _userRepository.GetSim((Guid)record.SimId);
                    var device = _userRepository.GetDevice((Guid)record.DeviceId);
                    if (record.StartDate < device.StartDate || record.StartDate > device.EndDate)
                        ModelState.AddModelError(nameof(record.StartDate), Messages.OutOfDate);
                    if (record.EndDate < device.StartDate || record.EndDate > device.EndDate)
                        ModelState.AddModelError(nameof(record.EndDate), Messages.OutOfDate);
                    if (!ModelState.IsValid)
                        return ValidationProblem(ModelState);

                    var simDevice = _context.SimAndDevice
                        .Include(s => s.Sim)
                        .Include(s => s.Device)
                        .Include(s => s.Sim.SimGroup)
                        .Include(s => s.Sim.SimGroup.Organization)
                        .SingleOrDefault(s => s.SimId == sim.Id && s.DeviceId == device.Id);
                    if (simDevice == null) // simDeviceはない　そしてDeleteはDかdのない　登録
                    {
                        var simDevice1 = new SimAndDevice()
                        {
                            Sim = sim,
                            Device = device,
                            AuthenticationDuration = (int)record.AuthenticationDuration,
                            IsolatedNw2Ip = record.IsolatedNw2Ip,
                            EndDate = record.EndDate,
                            StartDate = (DateTime)record.StartDate
                        };
                        _userRepository.Create(simDevice1);
                        simDevices.Add(simDevice1);
                        continue;
                    }
                    if (endUser != null && simDevice.Sim.SimGroup.Organization.Code != endUser.Domain.Organization.Code)
                    {
                        ModelState.AddModelError("Role", Messages.InvalidRole);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
                    }
                    if ("d".Equals(record.Delete) || "D".Equals(record.Delete)) // Dかdか そしてsimDeviceはあり　は削除
                    {
                        if (_context.SimAndDeviceAuthenticated.Any(s => s.SimAndDeviceId == simDevice.Id))
                        {
                            ModelState.AddModelError(nameof(SimAndDeviceAuthenticated), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.MultiFactor.Any(f => f.SimAndDeviceId == simDevice.Id))
                        {
                            ModelState.AddModelError("FactorCombination", Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.MultiFactorAuthenticationFailureLog.Any(f => f.SimAndDevice.Id == simDevice.Id))
                        {
                            ModelState.AddModelError("MultiFactorAuthenticationFailure", Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.SimAndDeviceAuthenticationSuccessLog.Any(f => f.SimAndDevice.Id == simDevice.Id))
                        {
                            ModelState.AddModelError("SimAndDeviceAuthenticationSuccess", Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        _userRepository.RemoveSimAndDevice(simDevice.Id);
                        simDevices.Add(simDevice);
                        continue;
                    }

                    simDevice.IsolatedNw2Ip = record.IsolatedNw2Ip;
                    simDevice.StartDate = (DateTime)record.StartDate;
                    simDevice.EndDate = record.EndDate;
                    simDevice.AuthenticationDuration = (int)record.AuthenticationDuration;
                    _userRepository.Update(simDevice);
                    simDevices.Add(simDevice);
                }

                transaction.Commit();
            }
            return Ok(simDevices);
        }

        /// <summary>
        /// CSVインポートのための型
        /// </summary>
        public sealed class CsvRecord : IValidatableObject
        {
            [Required]
            public Guid? SimId { get; set; }

            public string Delete { get; set; }

            [Required]
            public Guid? DeviceId { get; set; }

            [Required]
            [RegularExpression("^(?:(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\/([1-9]|[1-2]\\d|3[0-2])$", ErrorMessage = Messages.InvalidCIDR)]
            public string IsolatedNw2Ip { get; set; }

            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? EndDate { get; set; }

            [Required]
            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? StartDate { get; set; }

            [Required]
            public int? AuthenticationDuration { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (StartDate > EndDate)
                {
                    yield return new ValidationResult(Messages.InvalidEndDate, new[] { nameof(EndDate) });
                }
            }
        }

        /// <summary>
        /// CSVインポートのためのマップ
        /// </summary>
        public sealed class CsvRecordMap : ClassMap<CsvRecord>
        {
            public CsvRecordMap()
            {
                Map(a => a.Delete).Name("is Delete");
                Map(a => a.SimId).Name("SIM ID");
                Map(a => a.DeviceId).Name("Device ID");
                Map(a => a.IsolatedNw2Ip).Name("Isolated NW2 IP");
                Map(a => a.AuthenticationDuration).Name("Authentication Duration");
                Map(a => a.StartDate).Name("Start Date");
                Map(a => a.EndDate).Name("End Date");
            }
        }

        public class PutSimAndDeviceParam : IValidatableObject
        {
            [Required]
            [RegularExpression("^(?:(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}(?:[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\/([1-9]|[1-2]\\d|3[0-2])$", ErrorMessage = Messages.InvalidCIDR)]
            public string IsolatedNw2Ip { get; set; }

            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? EndDate { get; set; }

            [Required]
            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? StartDate { get; set; }

            [Required]
            public int? AuthenticationDuration { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (StartDate > EndDate)
                {
                    yield return new ValidationResult(Messages.InvalidEndDate, new[] { nameof(EndDate) });
                }
            }
        }

        public class RegisterParam : PutSimAndDeviceParam
        {
            [Required]
            public Guid? SimId { get; set; }

            [Required]
            public Guid? DeviceId { get; set; }
        }

        public enum SortKey
        {
            Msisdn,
            DeviceName,
            IsolatedNw2Ip,
            StartDate,
            EndDate,
            AuthenticationDuration,
        }

        public class GetSimAndDevicesParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Msisdn;
            public Order OrderBy { get; set; } = Order.Asc;

            public string Msisdn { get; set; }

            public string DeviceName { get; set; }

            public DateTime? StartDateFrom { get; set; }
            public DateTime? StartDateTo { get; set; }
            public DateTime? EndDateFrom { get; set; }
            public DateTime? EndDateTo { get; set; }
            public bool IncludingEndDateNotSet { get; set; } = false;
            public bool? IsAuthenticationDone { get; set; }
        }

        public class GetSimAndDevicesAdminParam : GetSimAndDevicesParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetSimAndDevicesAdminParam()
            {
            }

            public GetSimAndDevicesAdminParam(GetSimAndDevicesParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
