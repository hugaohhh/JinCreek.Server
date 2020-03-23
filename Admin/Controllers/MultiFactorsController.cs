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
    /// 認証要素組合せ
    /// GET: api/multi-factors                認証要素組合せ一覧照会
    /// GET: api/multi-factors/mine           自分認証要素組合せ一覧照会
    /// GET: api/multi-factors/{id}           認証要素組合せ照会
    /// GET: api/multi-factors/mine/{id}      自分認証要素組合せ照会
    /// POST: api/multi-factors               認証要素組合せ登録
    /// POST: api/multi-factors/mine          自分認証要素組合せ登録
    /// PUT: api/multi-factors                認証要素組合せ更新
    /// PUT: api/multi-factors/mine           自分認証要素組合せ更新
    /// DELETE: api/multi-factors/{id}        認証要素組合せ削除
    /// DELETE: api/multi-factors/mine/{id}   自分認証要素組合せ削除
    /// GET: api/multi-factors/csv            認証要素組合せエクスポート
    /// GET: api/multi-factors/mine/csv       自分認証要素組合せエクスポート
    /// POST: api/multi-factors/csv           認証要素組合せインポート
    /// POST: api/multi-factors/mine/csv      自分認証要素組合せインポート
    /// </summary>
    [Route("api/multi-factors")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "RedundantCatchClause")]
    public class MultiFactorsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public MultiFactorsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分認証要素組合せ登録
        /// POST: api/multi-factors/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine")]
        public ActionResult<MultiFactor> PostMultiFactorMine(RegisterParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            var simDevice = _userRepository.GetSimAndDevice((Guid)param.SimAndDeviceId);
            var endUser = _userRepository.GetEndUser((Guid)param.UserId);

            if (simDevice.Sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            if (simDevice.Device.Domain.OrganizationCode != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            if (endUser.Domain.OrganizationCode != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PostMultiFactor(param);
        }

        /// <summary>
        /// 認証要素組合せ登録
        /// POST: api/multi-factors
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult<MultiFactor> PostMultiFactor(RegisterParam param)
        {
            var simDevice = _userRepository.GetSimAndDevice((Guid)param.SimAndDeviceId);
            var endUser = _userRepository.GetEndUser((Guid)param.UserId);
            var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();

            if (param.StartDate < simDevice.StartDate || param.StartDate > simDevice.EndDate || availablePeriod?.StartDate > param.StartDate || availablePeriod?.EndDate < param.StartDate)
                ModelState.AddModelError(nameof(param.StartDate), Messages.OutOfDate);
            if (param.EndDate < simDevice.StartDate || param.EndDate > simDevice.EndDate || availablePeriod?.StartDate > param.EndDate || availablePeriod?.EndDate < param.EndDate)
                ModelState.AddModelError(nameof(param.EndDate), Messages.OutOfDate);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (_context.MultiFactor.Any(s => s.SimAndDevice.Id == simDevice.Id && s.EndUser.Id == endUser.Id))
                ModelState.AddModelError(nameof(MultiFactor), Messages.Duplicate);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var multiFactor = new MultiFactor()
            {
                SimAndDevice = simDevice,
                EndUser = endUser,
                StartDate = (DateTime)param.StartDate,
                EndDate = param.EndDate,
                ClosedNwIp = param.ClosedNwIp
            };
            try
            {
                _userRepository.Create(multiFactor);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return CreatedAtAction("GetMultiFactor", new { id = multiFactor.Id }, multiFactor);
        }

        /// <summary>
        /// 自分認証要素組合せ削除
        /// DELETE: api/multi-factors/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpDelete("mine/{id}")]
        public ActionResult<MultiFactor> DeleteMultiFactorMine(Guid id)
        {
            var multiFactor = _userRepository.GetMultiFactor(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (multiFactor.EndUser.Domain.OrganizationCode != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return DeleteMultiFactor(id);
        }

        /// <summary>
        /// 認証要素組合せ削除
        /// DELETE: api/multi-factors/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}")]
        public ActionResult<MultiFactor> DeleteMultiFactor(Guid id)
        {
            if (_context.DeauthenticationLog.Any(f => f.MultiFactor.Id == id))
                ModelState.AddModelError(nameof(DeauthenticationLog), Messages.ChildEntityExists);
            if (_context.MultiFactorAuthenticated.Any(m => m.MultiFactorId == id))
                ModelState.AddModelError(nameof(MultiFactorAuthenticated), Messages.ChildEntityExists);
            if (_context.MultiFactorAuthenticationSuccessLog.Any(f => f.MultiFactor.Id == id))
                ModelState.AddModelError(nameof(MultiFactorAuthenticationSuccessLog), Messages.ChildEntityExists);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var multiFactor = _userRepository.RemoveMultiFactor(id);
            return new ObjectResult(multiFactor) { StatusCode = StatusCodes.Status204NoContent };
        }

        /// <summary>
        /// 自分認証要素組合せ一覧照会
        /// GET: api/multi-factors/mine
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<MultiFactor>> GetMultiFactors(
            [FromQuery] GetFactorCombinationsParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetMultiFactors(new GetFactorCombinationsAdminParam(param)
            { OrganizationCode = user.Domain.OrganizationCode });
        }

        /// <summary>
        /// 認証要素組合せ一覧照会
        /// GET: api/multi-factors
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<MultiFactor>> GetMultiFactors(
            [FromQuery] GetFactorCombinationsAdminParam param)
        {
            // filter
            var query = _context.MultiFactor
                .Include(a => a.EndUser)
                .Include(a => a.SimAndDevice)
                .Include(a => a.SimAndDevice.Sim)
                .Include(a => a.SimAndDevice.Device)
                .Include(a => a.MultiFactorAuthenticated)
                .Where(a => a.EndUser.Domain.OrganizationCode == param.OrganizationCode);
            if (param.Msisdn != null) query = query.Where(a => a.SimAndDevice.Sim.Msisdn.Contains(param.Msisdn));
            if (param.DeviceName != null)
                query = query.Where(a => a.SimAndDevice.Device.Name.Contains(param.DeviceName));
            if (param.UserAccountName != null)
                query = query.Where(a => a.EndUser.AccountName.Contains(param.UserAccountName));
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
                query = (bool)param.IsAuthenticationDone
                    ? query.Where(a => a.MultiFactorAuthenticated != null)
                    : query.Where(a => a.MultiFactorAuthenticated == null);

            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.Msisdn)
                query = Utils.OrderBy(query, a => a.SimAndDevice.Sim.Msisdn, param.OrderBy)
                    .ThenBy(a => a.SimAndDevice.Device.Name).ThenBy(a => a.EndUser.AccountName);
            if (param.SortBy == SortKey.DeviceName)
                query = Utils.OrderBy(query, a => a.SimAndDevice.Device.Name, param.OrderBy).ThenBy(a => a.Id);
            if (param.SortBy == SortKey.UserAccountName)
                query = Utils.OrderBy(query, a => a.EndUser.AccountName, param.OrderBy).ThenBy(a => a.Id);
            if (param.SortBy == SortKey.ClosedNwIp)
                query = Utils.OrderBy(query, a => a.ClosedNwIp, param.OrderBy).ThenBy(a => a.Id);
            if (param.SortBy == SortKey.StartDate)
                query = Utils.OrderBy(query, a => a.StartDate, param.OrderBy).ThenBy(a => a.Id);
            if (param.SortBy == SortKey.EndDate)
                query = Utils.OrderBy(query, a => a.EndDate, param.OrderBy).ThenBy(a => a.Id);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<MultiFactor> { Count = count, Results = query.ToList() };
        }

        /// <summary>
        /// 自分 認証要素組合せ照会（ログインユーザーが所属する組織の認証要素組合せ一覧）
        /// GET: api/multi-factors/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/{id}")]
        public ActionResult<MultiFactor> GetMultiFactor(Guid id)
        {
            var multiFactor = _userRepository.GetMultiFactor(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (multiFactor.EndUser.Domain.OrganizationCode != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }

            return multiFactor;
        }

        /// <summary>
        /// 認証要素組合せ照会
        /// GET: api/multi-factors/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<MultiFactor> GetMultiFactor2(Guid id)
        {
            var multiFactor = _userRepository.GetMultiFactor(id);
            return multiFactor;
        }

        /// <summary>
        /// 自分認証要素組合せ更新
        /// PUT: api/multi-factors/mine
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine/{id}")]
        public IActionResult PutMultiFactorMine(Guid id, PutMultiFactorParam param)
        {
            var multiFactor = _userRepository.GetMultiFactor(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (multiFactor.EndUser.Domain.OrganizationCode != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PutMultiFactor(id, param);
        }

        /// <summary>
        /// 認証要素組合せ更新
        /// PUT: api/multi-factors
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public IActionResult PutMultiFactor(Guid id, PutMultiFactorParam param)
        {
            var multiFactor = _userRepository.GetMultiFactor(id);
            var availablePeriod = multiFactor.EndUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();

            if (param.StartDate < multiFactor.SimAndDevice.StartDate || param.StartDate > multiFactor.SimAndDevice.EndDate || availablePeriod?.StartDate > param.StartDate || availablePeriod?.EndDate < param.StartDate)
                ModelState.AddModelError(nameof(param.StartDate), Messages.OutOfDate);
            if (param.EndDate < multiFactor.SimAndDevice.StartDate || param.EndDate > multiFactor.SimAndDevice.EndDate || availablePeriod?.StartDate > param.EndDate || availablePeriod?.EndDate < param.EndDate)
                ModelState.AddModelError(nameof(param.EndDate), Messages.OutOfDate);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            multiFactor.StartDate = (DateTime)param.StartDate;
            multiFactor.EndDate = param.EndDate;
            multiFactor.ClosedNwIp = param.ClosedNwIp;
            _userRepository.Update(multiFactor);
            return Ok(multiFactor);
        }

        public class PutMultiFactorParam : IValidatableObject
        {
            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? EndDate { get; set; }

            [Required]
            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? StartDate { get; set; }

            [Required]
            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}",
                ErrorMessage = Messages.InvalidIpAddress)]
            public string ClosedNwIp { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (StartDate > EndDate)
                {
                    yield return new ValidationResult(Messages.InvalidEndDate, new[] { nameof(EndDate) });
                }
            }
        }

        public class RegisterParam : PutMultiFactorParam
        {
            [Required] public Guid? SimAndDeviceId { get; set; }
            [Required] public Guid? UserId { get; set; }
        }

        /// <summary>
        /// 自分認証要素組合せエクスポート
        /// GET: api/multi-factors/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/csv")]
        public ActionResult<string> GetMultiFactorsCsv()
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetMultiFactorsCsv(user.Domain.Organization.Code);
        }

        /// <summary>
        /// 認証要素組合せエクスポート
        /// GET: api/multi-factors/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("csv")]
        public ActionResult<string> GetMultiFactorsCsv([FromQuery] [Required] long organizationCode)
        {
            var multiFactors = _context.MultiFactor
                .Include(a => a.SimAndDevice)
                .Include(a => a.SimAndDevice.Sim)
                .Include(a => a.SimAndDevice.Device)
                .Include(a => a.EndUser)
                .Include(a => a.EndUser.Domain)
                .Include(a => a.EndUser.Domain.Organization)
                .Where(a => a.EndUser.Domain.OrganizationCode == organizationCode)
                .OrderBy(a => a.SimAndDevice.Sim.Msisdn)
                .ThenBy(a => a.SimAndDevice.Device.Name)
                .ThenBy(a => a.EndUser.AccountName)
                .ToList();

            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.Configuration.RegisterClassMap<MultiFactorMap>();
            csvWriter.WriteRecords(multiFactors);
            return writer.ToString();
        }

        /// <summary>
        /// CSVエクスポートのためのマップ
        /// </summary>
        public sealed class MultiFactorMap : ClassMap<MultiFactor>
        {
            public MultiFactorMap()
            {
                Map().Name("is Delete").Index(0).ConvertUsing(a => "");
                Map(a => a.SimAndDeviceId).Name("Sim And Device ID").Index(1);
                Map(a => a.SimAndDevice.Sim.Msisdn).Name("MSISDN").Index(2);
                Map(a => a.EndUser.Domain.Name).Name("Domain").Index(3);
                Map(a => a.SimAndDevice.Device.Name).Name("Device").Index(4);
                Map(a => a.EndUserId).Name("User ID").Index(5);
                Map(a => a.EndUser.AccountName).Name("User Name").Index(6);
                Map(a => a.ClosedNwIp).Name("Closed NW IP").Index(7);
                Map(a => a.StartDate).Name("Start Date").Index(8).TypeConverterOption.Format("yyyy-MM-dd");
                Map(a => a.EndDate).Name("End Date").Index(9).TypeConverterOption.Format("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 自分認証要素組合せインポート
        /// POST: api/multi-factors/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine/csv")]
        public ActionResult<IEnumerable<MultiFactor>> PostMultiFactorsCsv(IFormFile csv)
        {
            using var reader = new StreamReader(csv.OpenReadStream());
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return ImportCsv(reader, user);
        }

        /// <summary>
        /// 認証要素組合せインポート
        /// POST: api/multi-factors/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("csv")]
        public ActionResult<IEnumerable<MultiFactor>> PostMultiFactorsCsv2(IFormFile csv)
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
        private ActionResult<IEnumerable<MultiFactor>> ImportCsv(TextReader csv, EndUser endUser = null)
        {
            // 1. 型変換エラーがあればここで終了する：
            var records = Utils.ParseCsv<CsvRecord, CsvRecordMap>(csv, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // 2. バリデーションエラーがあればここで終了する：
            Utils.TryValidateCsvRecords(records, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var multiFactors = new List<MultiFactor>();
            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var record in records)
                {
                    var simDevice = _userRepository.GetSimAndDevice((Guid)record.SimAndDeviceId);
                    var user = _userRepository.GetEndUser((Guid)record.UserId);
                    var availablePeriod = user.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    if (record.StartDate < simDevice.StartDate || record.StartDate > simDevice.EndDate || availablePeriod?.StartDate > record.StartDate || availablePeriod?.EndDate < record.StartDate)
                        ModelState.AddModelError(nameof(record.StartDate), Messages.OutOfDate);
                    if (record.EndDate < simDevice.StartDate || record.EndDate > simDevice.EndDate || availablePeriod?.StartDate > record.EndDate || availablePeriod?.EndDate < record.EndDate)
                        ModelState.AddModelError(nameof(record.EndDate), Messages.OutOfDate);
                    if (!ModelState.IsValid)
                        return ValidationProblem(ModelState);

                    var multiFactor = _context.MultiFactor
                        .Include(m => m.SimAndDevice)
                        .Include(m => m.SimAndDevice.Sim)
                        .Include(m => m.SimAndDevice.Device)
                        .Include(m => m.MultiFactorAuthenticated)
                        .Include(m => m.EndUser)
                        .Include(m => m.EndUser.Domain)
                        .Include(m => m.EndUser.Domain.Organization)
                        .SingleOrDefault(m => m.SimAndDeviceId == simDevice.Id && m.EndUserId == user.Id);
                    if (multiFactor == null) // 多要素がない　そしてDeleteはDかdのない　登録
                    {
                        var multiFactor1 = new MultiFactor()
                        {
                            SimAndDevice = simDevice,
                            EndUser = user,
                            ClosedNwIp = record.ClosedNwIp,
                            EndDate = record.EndDate,
                            StartDate = (DateTime)record.StartDate
                        };
                        _userRepository.Create(multiFactor1);
                        multiFactors.Add(multiFactor1);
                        continue;
                    }

                    if (endUser != null && multiFactor.EndUser.Domain.OrganizationCode != endUser.Domain.Organization.Code)
                    {
                        ModelState.AddModelError("Role", Messages.InvalidRole);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
                    }

                    if ("d".Equals(record.Delete) || "D".Equals(record.Delete)) // Dかdか そして 多要素はあり　は削除
                    {
                        if (_context.MultiFactorAuthenticated.Any(m => m.MultiFactorId == multiFactor.Id))
                        {
                            ModelState.AddModelError(nameof(MultiFactorAuthenticated), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.DeauthenticationLog.Any(f => f.MultiFactor.Id == multiFactor.Id))
                        {
                            ModelState.AddModelError(nameof(DeauthenticationLog), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }

                        if (_context.MultiFactorAuthenticationSuccessLog.Any(f => f.MultiFactor.Id == multiFactor.Id))
                        {
                            ModelState.AddModelError(nameof(MultiFactorAuthenticationSuccessLog), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        _userRepository.RemoveMultiFactor(multiFactor.Id);
                        multiFactors.Add(multiFactor);
                        continue;
                    }
                    //更新
                    multiFactor.ClosedNwIp = record.ClosedNwIp;
                    multiFactor.StartDate = (DateTime)record.StartDate;
                    multiFactor.EndDate = record.EndDate;
                    _userRepository.Update(multiFactor);
                    multiFactors.Add(multiFactor);
                }

                transaction.Commit();
            }

            return Ok(multiFactors);
        }

        /// <summary>
        /// CSVインポートのための型
        /// </summary>
        public sealed class CsvRecord : IValidatableObject
        {
            public string Delete { get; set; }

            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? EndDate { get; set; }

            [Required]
            [JsonConverter(typeof(DateWithoutTimeConverter))]
            public DateTime? StartDate { get; set; }

            [Required]
            [RegularExpression("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}",
                ErrorMessage = Messages.InvalidIpAddress)]
            public string ClosedNwIp { get; set; }

            [Required]
            public Guid? SimAndDeviceId { get; set; }

            [Required]
            public Guid? UserId { get; set; }

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
                Map(a => a.SimAndDeviceId).Name("Sim And Device ID");
                Map(a => a.UserId).Name("User ID");
                Map(a => a.ClosedNwIp).Name("Closed NW IP");
                Map(a => a.StartDate).Name("Start Date");
                Map(a => a.EndDate).Name("End Date");
            }
        }

        public enum SortKey
        {
            Msisdn,
            DeviceName,
            UserAccountName,
            ClosedNwIp,
            StartDate,
            EndDate
        }

        public class GetFactorCombinationsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.Msisdn;
            public Order OrderBy { get; set; } = Order.Asc;

            public string UserAccountName { get; set; }
            public string Msisdn { get; set; }
            public string DeviceName { get; set; }
            public bool? IsAuthenticationDone { get; set; }

            public DateTime? StartDateFrom { get; set; }
            public DateTime? StartDateTo { get; set; }
            public DateTime? EndDateFrom { get; set; }
            public DateTime? EndDateTo { get; set; }
            public bool IncludingEndDateNotSet { get; set; } = false;
        }

        public class GetFactorCombinationsAdminParam : GetFactorCombinationsParam
        {
            [Required] public int? OrganizationCode { get; set; }

            public GetFactorCombinationsAdminParam()
            {
            }

            public GetFactorCombinationsAdminParam(GetFactorCombinationsParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
