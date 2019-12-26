﻿using Microsoft.AspNetCore.Identity;
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
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _appSettings;

        public AuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]UsersRegisterRequest request)
        {
            var result = await _userManager.CreateAsync(new IdentityUser { UserName = request.UserName }, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]UsersLoginRequest request)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, true, true);
            if (!signInResult.Succeeded)
            {
                return BadRequest("Invalid user name or password");
            }
            var user = await _userManager.FindByNameAsync(request.UserName);
            return Ok(new
            {
                AccessToken = GetToken(user.Id, _appSettings.AccessTokenExpiration),
                RefreshToken = GetToken(user.Id, _appSettings.RefreshTokenExpiration)
            });
        }

        [HttpPost("Refresh")]
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
                AccessToken = GetToken(user.Id, _appSettings.AccessTokenExpiration)
            });
        }

        private String GetToken(String id, Int32 expiration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var sKey = new SymmetricSecurityKey(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, id),
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
        public String UserName { get; set; }
        [Required]
        public String Password { get; set; }
    }

    public class UsersLoginRequest
    {
        [Required]
        public String UserName { get; set; }
        [Required]
        public String Password { get; set; }
    }

    public class UsersRefreshRequest
    {
        public String AccessToken { get; set; }
        [Required]
        public String RefreshToken { get; set; }
    }
}
