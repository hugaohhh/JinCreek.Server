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
    /// GET: api/devices/mine      自分端末一覧照会
    /// GET: api/devices           端末一覧照会
    /// GET: api/devices/mine/{id} 自分端末照会
    /// GET: api/devices/{id}      端末照会
    /// PUT: api/devices/mine/{id} 自分端末更新
    /// PUT: api/devices/{id}      端末更新
    /// GET: api/devices/mine/csv  自分端末エクスポート
    /// GET: api/devices/csv       端末エクスポート
    /// POST: api/devices/mine/csv 自分端末インポート
    /// POST: api/devices/csv      端末インポート
    /// </summary>
    [Route("api/devices")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DevicesController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DevicesController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分端末一覧照会（ログインユーザーが所属する組織の端末一覧）
        /// GET: api/devices/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<DeviceDto>> GetDevices([FromQuery] GetDevicesParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetDevices(new GetDevicesAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        /// <summary>
        /// 端末一覧照会
        /// GET: api/devices?organization=5
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<DeviceDto>> GetDevices([FromQuery] GetDevicesAdminParam param)
        {
            // filter
            var query = _context.Device
                .Include(a => a.DeviceGroupDevices).ThenInclude(a => a.DeviceGroup)
                .Include(a => a.Domain)
                .Include(a => a.LteModule)
                .Include(a => a.OrganizationClientApp.ClientApp.ClientOs)
                .Where(a => a.Domain.Organization.Code == param.OrganizationCode);
            if (param.DomainId != null) query = query.Where(a => a.Domain.Id == param.DomainId);
            if (param.DeviceGroupId != null) query = query.Where(a => a.DeviceGroupDevices.Any(b => b.DeviceGroupId == param.DeviceGroupId));
            if (param.Name != null) query = query.Where(a => a.Name.Contains(param.Name));
            if (param.ProductName != null) query = query.Where(a => a.ProductName.Contains(param.ProductName));
            if (param.LteModuleId != null) query = query.Where(a => a.LteModule.Id == param.LteModuleId);
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
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.DomainName) query = Utils.OrderBy(query, a => a.Domain.Name, param.OrderBy).ThenBy(a => a.Name);
            if (param.SortBy == SortKey.Name) query = Utils.OrderBy(query, a => a.Name, param.OrderBy);
            if (param.SortBy == SortKey.ProductName) query = Utils.OrderBy(query, a => a.ProductName, param.OrderBy).ThenBy(a => a.Domain.Name).ThenBy(a => a.Name);
            if (param.SortBy == SortKey.LteModuleName) query = Utils.OrderBy(query, a => a.LteModule.Name, param.OrderBy).ThenBy(a => a.Domain.Name).ThenBy(a => a.Name);
            if (param.SortBy == SortKey.StartDate)
                query = Utils.OrderBy(query, a => a.StartDate, param.OrderBy);
            if (param.SortBy == SortKey.EndDate)
                query = Utils.OrderBy(query, a => a.EndDate, param.OrderBy);
            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<DeviceDto> { Count = count, Results = query.Select(a => new DeviceDto(a)) };
        }

        /// <summary>
        /// 自分端末照会（ログインユーザーが所属する組織の端末一覧）
        /// GET: api/devices/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/{id}")]
        public ActionResult<DeviceDto> GetDevice(Guid id)
        {
            var device = _userRepository.GetDevice(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (device.Domain.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return new DeviceDto(device);
        }

        /// <summary>
        /// 端末照会
        /// GET: api/devices/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<DeviceDto> GetDevice2(Guid id)
        {
            return new DeviceDto(_userRepository.GetDevice(id));
        }

        /// <summary>
        /// 自分端末更新
        /// PUT: api/devices/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine/{id}")]
        public ActionResult<DeviceDto> PutDeviceMine(Guid id, PutDeviceParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            var device = _userRepository.GetDevice(id);
            if (device.Domain.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PutDevice(id, param);
        }

        /// <summary>
        /// 端末更新
        /// PUT: api/devices/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public ActionResult<DeviceDto> PutDevice(Guid id, PutDeviceParam param)
        {
            var device = _userRepository.GetDevice(id);

            device.ManagedNumber = param.ManagedNumber;
            device.SerialNumber = param.SerialNumber;
            device.ProductName = param.ProductName;
            device.LteModule = param.LteModuleId == null ? null : _userRepository.GetLteModule((Guid)param.LteModuleId);
            device.OrganizationClientApp = param.OrganizationClientAppId == null ? null : _userRepository.GetOrganizationClientApp((Guid)param.OrganizationClientAppId);
            device.UseTpm = (bool)param.UseTpm;
            device.WindowsSignInListCacheDays = param.WindowsSignInListCacheDays;

            _userRepository.Update(device);
            return Ok(new DeviceDto(_userRepository.GetDevice(id)));
        }

        public class DeviceDto
        {
            public Guid Id { get; set; }
            public Guid DomainId { get; set; }
            public Domain Domain { get; set; }
            public string Name { get; set; }
            public IEnumerable<DeviceGroup> DeviceGroups { get; set; }
            public string ManagedNumber { get; set; }
            public string SerialNumber { get; set; }
            public string ProductName { get; set; }
            public LteModule LteModule { get; set; }
            public OrganizationClientApp OrganizationClientApp { get; set; }
            public bool UseTpm { get; set; }
            public int? WindowsSignInListCacheDays { get; set; }
            [JsonConverter(typeof(DateWithoutTimeConverter))] public DateTime StartDate { get; set; }
            [JsonConverter(typeof(DateWithoutTimeConverter))] public DateTime? EndDate { get; set; }

            public DeviceDto(Device device)
            {
                Id = device.Id;
                DomainId = device.DomainId;
                Domain = device.Domain;
                Name = device.Name;
                StartDate = device.StartDate;
                EndDate = device.EndDate;
                DeviceGroups = device.DeviceGroupDevices?.Select(a => new DeviceGroup
                {
                    Id = a.DeviceGroup.Id,
                    Domain = a.DeviceGroup.Domain,
                    Name = a.DeviceGroup.Name
                });
                ManagedNumber = device.ManagedNumber;
                SerialNumber = device.SerialNumber;
                ProductName = device.ProductName;
                LteModule = device.LteModule;
                OrganizationClientApp = device.OrganizationClientApp;
                UseTpm = device.UseTpm;
                WindowsSignInListCacheDays = device.WindowsSignInListCacheDays;
            }
        }

        public class PutDeviceParam
        {
            //public string Name { get; set; } // 名前（更新しない）
            public string ManagedNumber { get; set; } // 社内管理番号 (nullable)
            public string SerialNumber { get; set; } // 製造番号 (nullable)
            public string ProductName { get; set; } // 機種名 (nullable)
            public Guid? LteModuleId { get; set; } // LTEモジュールID (nullable)
            public Guid? OrganizationClientAppId { get; set; } // 組織端末アプリID (nullable)
            [Required] public bool? UseTpm { get; set; } // TPM利用
            [Required] public int? WindowsSignInListCacheDays { get; set; } // Windowsサインイン許可リストキャッシュ日数
            //public DateTime? StartDate { get; set; } // 利用開始日（更新しない）
            //public DateTime? EndDate { get; set; } // 利用終了日（更新しない）
        }

        public enum SortKey
        {
            DomainName,
            Name,
            ProductName,
            LteModuleName,
            StartDate,
            EndDate
        }

        public class GetDevicesParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.DomainName;
            public Order OrderBy { get; set; } = Order.Asc;

            public Guid? DomainId { get; set; }
            public Guid? DeviceGroupId { get; set; }
            public string Name { get; set; }
            public string ProductName { get; set; }
            public Guid? LteModuleId { get; set; }
            public DateTime? StartDateFrom { get; set; }
            public DateTime? StartDateTo { get; set; }
            public DateTime? EndDateFrom { get; set; }
            public DateTime? EndDateTo { get; set; }
            public bool IncludingEndDateNotSet { get; set; } = false;
        }

        public class GetDevicesAdminParam : GetDevicesParam
        {
            [Required] public long? OrganizationCode { get; set; }

            public GetDevicesAdminParam()
            {
            }

            public GetDevicesAdminParam(GetDevicesParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }


        /// <summary>
        /// 自分端末エクスポート
        /// GET: api/devices/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/csv")]
        public ActionResult<string> GetDevicesCsv()
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetDevicesCsv(user.Domain.Organization.Code);
        }

        /// <summary>
        /// 端末エクスポート
        /// GET: api/devices/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("csv")]
        public ActionResult<string> GetDevicesCsv([FromQuery] [Required] long organizationCode)
        {
            var devices = _context.Device
                .Include(a => a.DeviceGroupDevices).ThenInclude(a => a.DeviceGroup)
                .Include(a => a.Domain)
                .Include(a => a.LteModule)
                .Include(a => a.OrganizationClientApp.ClientApp.ClientOs)
                .Where(a => a.Domain.Organization.Code == organizationCode)
                .OrderBy(a => a.Domain.Name)
                .ThenBy(a => a.Name)
                .ToList();

            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.Configuration.RegisterClassMap<DeviceMap>();
            csvWriter.WriteRecords(devices);

            return writer.ToString();
        }

        /// <summary>
        /// 自分端末インポート
        /// POST: api/devices/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine/csv")]
        public ActionResult<IEnumerable<Device>> PostDevicesCsv(IFormFile csv)
        {
            using var reader = new StreamReader(csv.OpenReadStream());
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return ImportCsv(reader, user);
        }

        /// <summary>
        /// 端末インポート
        /// POST: api/devices/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("csv")]
        public ActionResult<IEnumerable<Device>> PostDevicesCsv2(IFormFile csv)
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
        private ActionResult<IEnumerable<Device>> ImportCsv(TextReader csv, EndUser endUser = null)
        {
            // 1. 型変換エラーがあればここで終了する：
            var records = Utils.ParseCsv<CsvRecord, CsvRecordMap>(csv, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // 2. バリデーションエラーがあればここで終了する：
            Utils.TryValidateCsvRecords(records, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var devices = new List<Device>();
            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var record in records)
                {
                    var device = _context.Device
                        .Include(d => d.Domain).ThenInclude(a => a.Organization)
                        .FirstOrDefault(a => a.Id == record.Id);
                    if (device == null)
                    {
                        ModelState.AddModelError(nameof(Device), Messages.NotFound);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status404NotFound);
                    }
                    if (endUser != null && device.Domain.Organization.Code != endUser.Domain.Organization.Code)
                    {
                        ModelState.AddModelError("Role", Messages.InvalidRole);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
                    }

                    // 更新する：社内管理番号, 製造番号, 機種名, LTEモジュール, 組織端末アプリ, TPM利用, Windowsサインイン許可リストキャッシュ日数
                    // 更新しない：ID, ドメイン, 端末グループ, 名前, OS, バージョン, 利用開始日, 利用終了日
                    device.ManagedNumber = record.ManagedNumber;
                    device.SerialNumber = record.SerialNumber;
                    device.ProductName = record.ProductName;
                    device.LteModule = record.LteModuleId == null ? null : _userRepository.GetLteModule((Guid)record.LteModuleId);
                    device.OrganizationClientApp = record.OrganizationClientAppId == null ? null : _userRepository.GetOrganizationClientApp((Guid)record.OrganizationClientAppId);
                    device.UseTpm = (bool)record.UseTpm;
                    device.WindowsSignInListCacheDays = record.WindowsSignInListCacheDays;

                    _userRepository.Update(device);
                    devices.Add(device);
                }

                transaction.Commit();
            }
            return Ok(devices);
        }

        /// <summary>
        /// CSVインポートのための型
        /// </summary>
        public class CsvRecord
        {
            [Required] public Guid? Id { get; set; }
            public string DomainName { get; set; }
            public string Name { get; set; }
            public string ManagedNumber { get; set; }
            public string SerialNumber { get; set; }
            public string ProductName { get; set; }
            [Required] public Guid? LteModuleId { get; set; }
            [Required] public Guid? OrganizationClientAppId { get; set; }
            [Required] public bool? UseTpm { get; set; } = false;
            [Required] [Range(0, int.MaxValue)] public int? WindowsSignInListCacheDays { get; set; }
            //public DateTime? StartDate { get; set; } // 更新しない
            //public DateTime? EndDate { get; set; } // 更新しない
        }

        /// <summary>
        /// CSVインポートのためのマップ
        /// </summary>
        public sealed class CsvRecordMap : ClassMap<CsvRecord>
        {
            public CsvRecordMap()
            {
                Map(a => a.Id).Name("ID");
                Map(a => a.DomainName).Name("Domain");
                Map(a => a.Name).Name("Name");
                Map(a => a.ManagedNumber).Name("Managed Number");
                Map(a => a.SerialNumber).Name("Serial Number");
                Map(a => a.ProductName).Name("Product Name");
                Map(a => a.LteModuleId).Name("LTE Module ID");
                Map(a => a.OrganizationClientAppId).Name("JinCreek App ID");
                Map(a => a.UseTpm).Name("Use Tpm").Optional();
                Map(a => a.WindowsSignInListCacheDays).Name("OS SignIn List Cache Days");
                //Map(a => a.StartDate).Name("Start Date"); // 更新しない
                //Map(a => a.EndDate).Name("End Date"); // 更新しない
            }
        }

        /// <summary>
        /// CSVエクスポートのためのマップ
        /// </summary>
        public sealed class DeviceMap : ClassMap<Device>
        {
            public DeviceMap()
            {
                Map(a => a.Id).Name("ID").Index(0);
                Map(a => a.Domain.Name).Name("Domain").Index(1);
                Map(a => a.DeviceGroupDevices).Name("Device Group").Index(2).ConvertUsing(a =>
                    string.Join(",", a.DeviceGroupDevices.Select(b => b.DeviceGroup.Name).OrderBy(c => c))); // カンマ区切り; 端末グループ名にカンマが入ってる場合を考慮しない
                Map(a => a.Name).Name("Name").Index(3);
                Map(a => a.ManagedNumber).Name("Managed Number").Index(4);
                Map(a => a.SerialNumber).Name("Serial Number").Index(5);
                Map(a => a.ProductName).Name("Product Name").Index(6);
                Map(a => a.LteModule.Id).Name("LTE Module ID").Index(7);
                Map(a => a.LteModule.Name).Name("LTE Module").Index(8);
                Map(a => a.OrganizationClientApp.Id).Name("JinCreek App ID").Index(9);
                Map(a => a.OrganizationClientApp.ClientApp.ClientOs.Name).Name("OS").Index(10);
                Map(a => a.OrganizationClientApp.ClientApp.Version).Name("JinCreek App Version").Index(11);
                //Map(a => a.UseTpm).Name("Use Tpm").Index(12);
                Map(a => a.WindowsSignInListCacheDays).Name("OS SignIn List Cache Days").Index(13);
                Map(a => a.StartDate).Name("Start Date").Index(14).TypeConverterOption.Format("yyyy-MM-dd");
                Map(a => a.EndDate).Name("End Date").Index(15).TypeConverterOption.Format("yyyy-MM-dd");
            }
        }
    }
}
