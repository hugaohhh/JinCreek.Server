using CsvHelper;
using CsvHelper.Configuration;
using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// SIM管理
    /// POST api/sims               SIM登録
    /// POST api/sims/mine          自分SIM登録
    /// GET: api/sims               SIM一覧照会
    /// GET: api/sims/mine          自分SIM一覧照会
    /// GET: api/sims/{id}          SIM照会
    /// GET: api/sims/mine/{id}     自分SIM照会
    /// PUT: api/sims/{id}          SIM更新
    /// PUT: api/sims/mine/{id}     自分SIM更新
    /// GET: api/sims/csv           SIMエクスポート
    /// GET: api/sims/mine/csv      自分SIMエクスポート
    /// POST: api/sims/csv          SIMインポート
    /// POST: api/sims/mine/csv     自分SIMインポート
    /// DELETE: api/sims/{id}       SIM削除
    /// DELETE: api/sims/mine/{id}  自分SIM削除
    /// </summary>
    [Route("api/sims")]
    [ApiController]
    [Authorize]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SimsController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public SimsController(UserRepository userRepository, MainDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// 自分SIM登録
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine")]
        public ActionResult<Sim> PostSimMine(RegisterParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            var simGroup = _userRepository.GetSimGroup((Guid)param.SimGroupId);
            if (user.Domain.Organization.Code != simGroup.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PostSim(param);
        }

        /// <summary>
        /// SIM登録
        /// POST api/sims/
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult<Sim> PostSim(RegisterParam param)
        {
            var simGroup = _userRepository.GetSimGroup((Guid)param.SimGroupId);
            if (_context.Sim.Any(s => s.UserName == param.UserName && s.SimGroup.UserNameSuffix == simGroup.UserNameSuffix))
            {
                ModelState.AddModelError(nameof(Sim.UserName), Messages.Duplicate);
                return ValidationProblem(ModelState);
            }

            if (param.UserName == null || "".Equals(param.UserName))
            {
                param.UserName = GenerateRandomPassword(16);
                while (_context.Sim.Any(s => s.UserName == param.UserName && s.SimGroup.UserNameSuffix == simGroup.UserNameSuffix))
                {
                    param.UserName = GenerateRandomPassword(16);
                }
            }
            if (param.Password == null || "".Equals(param.Password))
            {
                param.Password = GenerateRandomPassword(32);
            }
            var sim = new Sim()
            {
                Msisdn = param.Msisdn,
                Imsi = param.Imsi,
                IccId = param.IccId,
                UserName = param.UserName,
                Password = param.Password,
                SimGroup = simGroup
            };
            try
            {
                _userRepository.Create(sim);
            }
            catch (DbUpdateException)
            {
                if (_context.Sim.Any(s => s.Msisdn == param.Msisdn))
                    ModelState.AddModelError(nameof(Sim.Msisdn), Messages.Duplicate);
                if (_context.Sim.Any(s => s.IccId == param.IccId))
                    ModelState.AddModelError(nameof(Sim.IccId), Messages.Duplicate);
                if (_context.Sim.Any(s => s.Imsi == param.Imsi))
                    ModelState.AddModelError(nameof(Sim.Imsi), Messages.Duplicate);
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);
                throw;
            }
            return CreatedAtAction("GetSim", new { id = sim.Id }, sim);
        }

        const string PWS_CHARS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string GenerateRandomPassword(int length, string availableChars = PWS_CHARS)
        {
            if (string.IsNullOrEmpty(availableChars)) availableChars = PWS_CHARS;
            var r = new Random();
            return string.Join("", Enumerable.Range(0, length).Select(_ => PWS_CHARS[r.Next(availableChars.Length)]));
        }

        /// <summary>
        /// 自分SIM削除
        /// DELETE api/sims/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpDelete("mine/{id}")]
        public ActionResult<Sim> DeleteSimMine(Guid id)
        {
            var sim = _userRepository.GetSim(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return DeleteSim(id);
        }

        /// <summary>
        /// SIM削除
        /// DELETE api/sims/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpDelete("{id}")]
        public ActionResult<Sim> DeleteSim(Guid id)
        {
            try
            {
                Sim sim = _userRepository.RemoveSim(id);
                return new ObjectResult(sim) { StatusCode = StatusCodes.Status204NoContent };
            }
            catch (DbUpdateException)
            {
                if (_context.SimAndDevice.Any(s => s.SimId == id))
                    ModelState.AddModelError(nameof(SimAndDevice), Messages.ChildEntityExists);
                if (_context.SimAndDeviceAuthenticationFailureLog.Any(s => s.Sim.Id == id))
                    ModelState.AddModelError(nameof(SimAndDeviceAuthenticationFailureLog), Messages.ChildEntityExists);
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);
                throw;
            }
        }

        /// <summary>
        /// 自分SIM一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine")]
        public ActionResult<PaginatedResponse<Sim>> GetSims([FromQuery] GetSimsParam param)
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetSims(new GetSimsAdminParam(param) { OrganizationCode = user.Domain.Organization.Code });
        }

        /// <summary>
        /// SIM一覧照会
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult<PaginatedResponse<Sim>> GetSims([FromQuery] GetSimsAdminParam param)
        {
            // filter
            var query = _context.Sim
                .Include(a => a.SimGroup)
                .Where(a => a.SimGroup.Organization.Code == param.OrganizationCode);
            if (param.UserName != null) query = query.Where(a => a.UserName.Contains(param.UserName));
            if (param.Msisdn != null) query = query.Where(a => a.Msisdn.Contains(param.Msisdn));
            if (param.SimGroupId != null) query = query.Where(a => a.SimGroup.Id == param.SimGroupId);
            var count = query.Count();

            // ordering
            if (param.SortBy == SortKey.SimGroupName) query = Utils.OrderBy(query, a => a.SimGroup.Name, param.OrderBy).ThenBy(a => a.Msisdn);
            if (param.SortBy == SortKey.Msisdn) query = Utils.OrderBy(query, a => a.Msisdn, param.OrderBy);
            if (param.SortBy == SortKey.UserName) query = Utils.OrderBy(query, a => a.UserName, param.OrderBy);

            // paging
            if (param.Page != null) query = query.Skip((int)((param.Page - 1) * param.PageSize)).Take(param.PageSize);

            return new PaginatedResponse<Sim> { Count = count, Results = query.Include(a => a.SimGroup).ToList() };
        }

        /// <summary>
        /// 自分Sim照会（ログインユーザーが所属する組織のSIm一覧）
        /// GET: api/sims/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/{id}")]
        public ActionResult<Sim> GetSim(Guid id)
        {
            var sim = _userRepository.GetSim(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return sim;
        }

        /// <summary>
        /// Sim照会
        /// GET: api/sims/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id}")]
        public ActionResult<Sim> GetSim2(Guid id)
        {
            return _userRepository.GetSim(id);
        }

        /// <summary>
        /// 自分Sim更新
        /// PUT: api/sims/mine/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPut("mine/{id}")]
        public IActionResult PutSimMine(Guid id, RegisterParam param)
        {
            var sim = _userRepository.GetSim(id);
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            if (sim.SimGroup.Organization.Code != user.Domain.Organization.Code)
            {
                ModelState.AddModelError("Role", Messages.InvalidRole);
                return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
            }
            return PutSim(id, param);
        }

        /// <summary>
        /// sim更新
        /// PUT: api/sims/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id}")]
        public IActionResult PutSim(Guid id, RegisterParam param)
        {
            var sim = _userRepository.GetSim(id);
            var simGroup = _userRepository.GetSimGroup((Guid)param.SimGroupId);
            if (_context.Sim.Any(s => s.Id != id && s.UserName == param.UserName && s.SimGroup.UserNameSuffix == sim.SimGroup.UserNameSuffix))
            {
                ModelState.AddModelError(nameof(Sim.UserName), Messages.Duplicate);
                return ValidationProblem(ModelState);
            }
            if (param.UserName == null || "".Equals(param.UserName))
            {
                param.UserName = GenerateRandomPassword(16);
                while (_context.Sim.Any(s => s.UserName == param.UserName && s.SimGroup.UserNameSuffix == sim.SimGroup.UserNameSuffix))
                {
                    param.UserName = GenerateRandomPassword(16);
                }
            }
            if (param.Password == null || "".Equals(param.Password))
            {
                param.Password = GenerateRandomPassword(32);
            }
            sim.Msisdn = param.Msisdn;
            sim.IccId = param.IccId;
            sim.Imsi = param.Imsi;
            sim.UserName = param.UserName;
            sim.Password = param.Password;
            sim.SimGroup = simGroup;
            try
            {
                _userRepository.Update(sim);
                return Ok(_userRepository.GetSim(id));
            }
            catch (DbUpdateException)
            {
                if (_context.Sim.Any(s => s.Id != id && s.Msisdn == param.Msisdn))
                    ModelState.AddModelError(nameof(Sim.Msisdn), Messages.Duplicate);
                if (_context.Sim.Any(s => s.Id != id && s.IccId == param.IccId))
                    ModelState.AddModelError(nameof(Sim.IccId), Messages.Duplicate);
                if (_context.Sim.Any(s => s.Id != id && s.Imsi == param.Imsi))
                    ModelState.AddModelError(nameof(Sim.Imsi), Messages.Duplicate);
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);
                throw;
            }
        }

        /// <summary>
        /// 自分Simエクスポート
        /// GET: api/sims/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpGet("mine/csv")]
        public ActionResult<string> GetSimsCsv()
        {
            var user = _userRepository.GetEndUser(Guid.Parse(User.Identity.Name));
            return GetSimsCsv(user.Domain.Organization.Code);
        }

        /// <summary>
        /// Simエクスポート
        /// GET: api/sims/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("csv")]
        public ActionResult<string> GetSimsCsv([FromQuery] [Required] long organizationCode)
        {
            var sims = _context.Sim
                .Include(a => a.SimGroup)
                .Include(a => a.SimGroup.Organization)
                .Where(a => a.SimGroup.Organization.Code == organizationCode)
                .OrderBy(a => a.SimGroup.Name)
                .ThenBy(a => a.UserName)
                .ToList();

            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.Configuration.RegisterClassMap<SimMap>();
            csvWriter.WriteRecords(sims);
            return writer.ToString();
        }

        public sealed class SimMap : ClassMap<Sim>
        {
            public SimMap()
            {
                Map().Name("is Delete").Index(0).ConvertUsing(a => "");
                Map(a => a.Id).Name("ID").Index(1);
                Map(a => a.SimGroup.Id).Name("SIM Group ID").Index(2);
                Map(a => a.SimGroup.Name).Name("SIM Group Name").Index(3);
                Map(a => a.Msisdn).Name("MSISDN").Index(4);
                Map(a => a.Imsi).Name("IMSI").Index(5);
                Map(a => a.IccId).Name("ICC ID").Index(6);
                Map(a => a.UserName).Name("User Name").Index(7);
                Map(a => a.Password).Name("Password").Index(8);
            }
        }

        /// <summary>
        /// 自分simインポート
        /// POST: api/sims/mine/csv
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Roles.UserAdmin)]
        [HttpPost("mine/csv")]
        public ActionResult<IEnumerable<Sim>> PostSimsCsv(IFormFile csv)
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
        public ActionResult<IEnumerable<Sim>> PostDevicesCsv2(IFormFile csv)
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
        private ActionResult<IEnumerable<Sim>> ImportCsv(TextReader csv, EndUser endUser = null)
        {
            // 1. 型変換エラーがあればここで終了する：
            var records = Utils.ParseCsv<CsvRecord, CsvRecordMap>(csv, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // 2. バリデーションエラーがあればここで終了する：
            Utils.TryValidateCsvRecords(records, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var sims = new List<Sim>();
            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var record in records)
                {
                    var simGroup = _userRepository.GetSimGroup(record.SimGroupId);
                    if (record.ID == null) // IDがない そしてDeleteはDかdのない　登録
                    {
                        if ("D".Equals(record.Delete) || "d".Equals(record.Delete))
                        {
                            ModelState.AddModelError("Id", Messages.InvalidSimId);
                            return ValidationProblem(ModelState);
                        }
                        //登録の重複チェック
                        if (_context.Sim.Any(s => s.Msisdn == record.Msisdn))
                        {
                            ModelState.AddModelError(nameof(Sim.Msisdn), Messages.Duplicate);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.Sim.Any(s => s.IccId == record.IccId))
                        {
                            ModelState.AddModelError(nameof(Sim.IccId), Messages.Duplicate);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.Sim.Any(s => s.Imsi == record.Imsi))
                        {
                            ModelState.AddModelError(nameof(Sim.Imsi), Messages.Duplicate);
                            return ValidationProblem(ModelState);
                        }
                        var sim1 = new Sim()
                        {
                            IccId = record.IccId,
                            Msisdn = record.Msisdn,
                            Imsi = record.Imsi,
                            UserName = record.UserName,
                            Password = record.Password,
                            SimGroup = simGroup
                        };
                        if (_context.Sim.Any(s => s.UserName == record.UserName && s.SimGroup.UserNameSuffix == simGroup.UserNameSuffix))
                        {
                            ModelState.AddModelError(nameof(Sim.UserName), Messages.Duplicate);
                            return ValidationProblem(ModelState);
                        }
                        _userRepository.Create(sim1);
                        sims.Add(sim1);
                        continue;
                    }
                    var sim = _context.Sim.Include(d => d.SimGroup).Include(s => s.SimGroup.Organization).FirstOrDefault(a => a.Id == record.ID);
                    if (sim == null)
                    {
                        ModelState.AddModelError(nameof(Sim), Messages.NotFound);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status404NotFound);
                    }
                    if (endUser != null && sim.SimGroup.Organization.Code != endUser.Domain.Organization.Code)
                    {
                        ModelState.AddModelError("Role", Messages.InvalidRole);
                        return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status403Forbidden);
                    }
                    if ("d".Equals(record.Delete) || "D".Equals(record.Delete)) // Dかdか そしてIDはあり　は削除
                    {
                        if (_context.SimAndDevice.Any(s => s.SimId == sim.Id))
                        {
                            ModelState.AddModelError(nameof(SimAndDevice), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        if (_context.SimAndDeviceAuthenticationFailureLog.Any(s => s.Sim.Id == sim.Id))
                        {
                            ModelState.AddModelError(nameof(SimAndDeviceAuthenticationFailureLog), Messages.ChildEntityExists);
                            return ValidationProblem(ModelState);
                        }
                        _userRepository.RemoveSim((Guid)record.ID);
                        sims.Add(sim);
                        continue;
                    }
                    // 更新の重複チェック
                    if (_context.Sim.Any(s => s.Id != record.ID && s.Msisdn == record.Msisdn))
                    {
                        ModelState.AddModelError(nameof(Sim.Msisdn), Messages.Duplicate);
                        return ValidationProblem(ModelState);
                    }
                    if (_context.Sim.Any(s => s.Id != record.ID && s.IccId == record.IccId))
                    {
                        ModelState.AddModelError(nameof(Sim.IccId), Messages.Duplicate);
                        return ValidationProblem(ModelState);
                    }
                    if (_context.Sim.Any(s => s.Id != record.ID && s.Imsi == record.Imsi))
                    {
                        ModelState.AddModelError(nameof(Sim.Imsi), Messages.Duplicate);
                        return ValidationProblem(ModelState);
                    }
                    if (_context.Sim.Any(s => s.Id != record.ID && s.UserName == record.UserName && s.SimGroup.UserNameSuffix == simGroup.UserNameSuffix))
                    {
                        ModelState.AddModelError(nameof(Sim.UserName), Messages.Duplicate);
                        return ValidationProblem(ModelState);
                    }
                    sim.UserName = record.UserName;
                    sim.Password = record.Password;
                    sim.Imsi = record.Imsi;
                    sim.IccId = record.IccId;
                    _userRepository.Update(sim);
                    sims.Add(sim);
                }

                transaction.Commit();
            }
            return Ok(sims);
        }

        public sealed class CsvRecord
        {
            public Guid? ID { get; set; }
            public string SimGroupName { get; set; }
            [Required]
            [StringLength(15)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "msisdn_is_only_number")]
            public string Msisdn { get; set; }
            [Required]
            [StringLength(15)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "imsi_is_only_number")]
            public string Imsi { get; set; }
            [Required]
            [StringLength(19)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "iccid_is_only_number")]
            public string IccId { get; set; }
            [Required]
            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = "UserName_is_only_ASCII")]
            public string UserName { get; set; }
            [Required]
            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = "Password_is_only_ASCII")]
            public string Password { get; set; }

            public string Delete { get; set; }

            public Guid SimGroupId { get; set; }
        }

        public sealed class CsvRecordMap : ClassMap<CsvRecord>
        {
            public CsvRecordMap()
            {
                Map(a => a.Delete).Name("is Delete");
                Map(a => a.ID).Name("ID");
                Map(a => a.SimGroupId).Name("SIM Group ID");
                Map(a => a.Msisdn).Name("MSISDN");
                Map(a => a.Imsi).Name("IMSI");
                Map(a => a.IccId).Name("ICC ID");
                Map(a => a.UserName).Name("User Name");
                Map(a => a.Password).Name("Password");
            }
        }

        public class PutSimParam
        {
            [Required]
            [StringLength(15)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "msisdn_is_only_number")]
            public string Msisdn { get; set; }
            [Required]
            [StringLength(15)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "imsi_is_only_number")]
            public string Imsi { get; set; }
            [Required]
            [StringLength(19)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "iccid_is_only_number")]
            public string IccId { get; set; }
            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = "UserName_is_only_ASCII")]
            public string UserName { get; set; }
            [RegularExpression("^[\\x21-\\x7eA-Za-z0-9]*$", ErrorMessage = "Password_is_only_ASCII")]
            public string Password { get; set; }

        }

        public class RegisterParam : PutSimParam
        {
            [Required]
            public Guid? SimGroupId { get; set; }
        }

        public enum SortKey
        {
            UserName,
            SimGroupName,
            Msisdn
        }

        public class GetSimsParam : PageParam
        {
            public SortKey SortBy { get; set; } = SortKey.SimGroupName;
            public Order OrderBy { get; set; } = Order.Asc;

            public string UserName { get; set; }
            public string Msisdn { get; set; }
            public Guid? SimGroupId { get; set; }
        }

        public class GetSimsAdminParam : GetSimsParam
        {
            [Required] public int? OrganizationCode { get; set; }

            public GetSimsAdminParam()
            {
            }

            public GetSimsAdminParam(GetSimsParam param)
            {
                foreach (var p in param.GetType().GetProperties())
                {
                    GetType().GetProperty(p.Name)?.SetValue(this, p.GetValue(param));
                }
            }
        }
    }
}
