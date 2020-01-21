using Admin.CustomProvider;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppSettings _appSettings;

        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// スーパー管理者を作る（テスト用）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UsersRegisterRequest request)
        {
            var result = await _userManager.CreateAsync(new ApplicationUser { UserName = request.UserName }, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UsersLoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return BadRequest("Invalid user name or password");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (!signInResult.Succeeded)
            {
                return BadRequest("Invalid user name or password");
            }

            return Ok(new
            {
                AccessToken = GetToken(user.Id.ToString(), user.Role, _appSettings.AccessTokenExpiration),
                RefreshToken = GetToken(user.Id.ToString(), user.Role, _appSettings.RefreshTokenExpiration)
            });
        }

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
                return BadRequest(new { title = e.Message });
            }

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid accessToken");
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

    public class UsersRegisterRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class UsersLoginRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class UsersRefreshRequest
    {
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
