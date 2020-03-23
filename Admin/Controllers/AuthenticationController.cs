using JinCreek.Server.Admin.CustomProvider;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JinCreek.Server.Admin.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppSettings _appSettings;
        private readonly MainDbContext _context;

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<AppSettings> appSettings, MainDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _context = context;
        }

        /// <summary>
        /// トークン取得
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UsersLoginRequest request)
        {
            User user;
            if (request.OrganizationCode == null && request.DomainName == null)
            {
                user = _context.SuperAdmin.AsNoTracking().FirstOrDefault(a => a.AccountName == request.UserName);
            }
            else
            {
                user = _context.EndUser.AsNoTracking()
                    .Include(a => a.Domain.Organization)
                    .Include(a => a.AvailablePeriods)
                    .FirstOrDefault(a => a.AccountName == request.UserName && a.Domain.OrganizationCode == request.OrganizationCode && a.Domain.Name == request.DomainName);
            }
            if (user == null)
            {
                ModelState.AddModelError("", Messages.InvalidUserNameOrPassword);
                return ValidationProblem(ModelState);
            }

            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            var signInResult = await _signInManager.CheckPasswordSignInAsync(applicationUser, request.Password, true);
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", Messages.InvalidUserNameOrPassword);
                return ValidationProblem(ModelState);
            }

            if (user is UserAdmin userAdmin)
            {
                var today = DateTime.Today;

                // 所属する組織が有効でない場合はエラー
                if (!(userAdmin.Domain.Organization.IsValid ?? false))
                {
                    ModelState.AddModelError(nameof(Organization), Messages.InvalidOrganization);
                    return ValidationProblem(ModelState);
                }

                // 所属する組織の利用日内でない場合はエラー
                if (today < userAdmin.Domain.Organization.StartDate || userAdmin.Domain.Organization.EndDate < today)
                {
                    ModelState.AddModelError(nameof(Organization), Messages.OutOfDateOrganization);
                    return ValidationProblem(ModelState);
                }

                // 当該ユーザーの利用日内でない場合はエラー
                // AvailablePeriodsのうちStartDayが最も後のものをとる。同日が複数取れる場合はソートが安定しない。
                var availablePeriod = userAdmin.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                if (today < availablePeriod?.StartDate || availablePeriod?.EndDate < today)
                {
                    ModelState.AddModelError(nameof(User), Messages.OutOfDateUser);
                    return ValidationProblem(ModelState);
                }
            }

            return Ok(new
            {
                AccessToken = GetToken(applicationUser.Id.ToString(), applicationUser.Role, _appSettings.AccessTokenExpiration),
                RefreshToken = GetToken(applicationUser.Id.ToString(), applicationUser.Role, _appSettings.RefreshTokenExpiration)
            });
        }

        /// <summary>
        /// トークンリフレッシュ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody]UsersRefreshRequest request)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret)),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(request.RefreshToken, tokenValidationParameters, out securityToken);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(request.RefreshToken), e.Message);
                return ValidationProblem(ModelState);
            }

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException(Messages.InvalidAccessToken);
            }
            if (principal == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(principal.Identity.Name);
            if (user == null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                AccessToken = GetToken(user.Id.ToString(), user.Role, _appSettings.AccessTokenExpiration)
            });
        }

        private string GetToken(string id, string role, int expiration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var sKey = new SymmetricSecurityKey(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, id),
                    new Claim(ClaimTypes.Role, role),
                }),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expiration),
                SigningCredentials = new SigningCredentials(sKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public class UsersLoginRequest
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public int? OrganizationCode { get; set; }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string DomainName { get; set; }
        [Required]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string UserName { get; set; }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required] public string Password { get; set; }
    }

    public class UsersRefreshRequest
    {
        public string AccessToken { get; set; }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public string RefreshToken { get; set; }
    }
}
