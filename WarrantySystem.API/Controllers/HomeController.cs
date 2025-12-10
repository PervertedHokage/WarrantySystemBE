using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WarrantySystem.API.Attributes;
using WarrantySystem.API.Middlewares;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        private readonly IConfiguration _configuration;
        private IGenericRepo _repo;

        public HomeController(IOptions<JwtSettings> jwtSettings, IConfiguration configuration, IGenericRepo repo)
        {
            _jwtSettings = jwtSettings.Value;
            _configuration = configuration;
            _repo = repo;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.LoginName) || string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    return Unauthorized(ApiResponseFactory.Fail(null, "Vui lòng nhập Tên đăng nhập và Mật khẩu!"));
                }

                //1. Check user
                string loginName = user.LoginName ?? "";
                string password = MaHoaMD5.EncryptPassword(user.PasswordHash ?? "");

                User? hasUser = await _repo.FindModel<User>(u => u.LoginName.ToLower().Trim() == loginName.ToLower().Trim() && u.PasswordHash == password);

                if (hasUser == null || hasUser.Id <= 0)
                {
                    return Unauthorized(ApiResponseFactory.Fail(null, "Sai tên đăng nhập hoặc mật khẩu!"));
                }

                //2. Tạo Claims
                var claims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.Sub,hasUser.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName,hasUser.LoginName ?? ""),
                };

                //var dictionary = (IDictionary<string, object>)hasUser;
                var properties = hasUser.GetType().GetProperties();

                foreach (var item in properties)
                {
                    //if (item.Key.ToLower() == "passwordhash") continue;
                    var claim = new Claim(item.Name.ToLower(), item.GetValue(hasUser)?.ToString() ?? "");
                    claims.Add(claim);
                }

                //3. Tạo token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims.ToArray(),
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                //4.Lưu session trên server
                HttpContext.Session.SetObject<CurrentUser>(_configuration.GetValue<string>("SessionKey"), ObjectMapper.GetCurrentUser(claims.ToDictionary(x => x.Type, x => x.Value)));

                return Ok(new
                {
                    access_token = tokenString,
                    expires = token.ValidTo.AddHours(+7)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        //[ApiKeyAuthorize]
        //[HttpPost("loginiden")]
        //public async Task< IActionResult> LoginIdentificaion([FromBody] User user)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(user.LoginName))
        //        {
        //            return Unauthorized(ApiResponseFactory.Fail(null, "Vui lòng nhập Tên đăng nhập!"));
        //        }

        //        //1. Check user
        //        string loginName = user.LoginName ?? "";
        //        string apiKey = _configuration.GetValue<string>("ApiKey") ?? "";

        //        User? hasUser = await _repo.FindModel<User>(u => u.LoginName.ToLower().Trim() == loginName.ToLower().Trim() && u.PasswordHash == password);

        //        if (hasUser == null || hasUser.Id <= 0)
        //        {
        //            return Unauthorized(ApiResponseFactory.Fail(null, "Sai tên đăng nhập hoặc mật khẩu!"));
        //        }
        //        var claims = new List<Claim>()
        //        {
        //            new Claim(JwtRegisteredClaimNames.Sub,hasUser.Id.ToString()),
        //            new Claim(JwtRegisteredClaimNames.UniqueName,hasUser.LoginName ?? ""),
        //        };
        //        var dictionary = (IDictionary<string, object>)hasUser;
        //        foreach (var item in dictionary)
        //        {
        //            //if (item.Key.ToLower() == "passwordhash") continue;

        //            var claim = new Claim(item.Key.ToLower(), item.Value?.ToString() ?? "");
        //            claims.Add(claim);
        //        }

        //        //3. Tạo token
        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //        var token = new JwtSecurityToken(
        //            issuer: _jwtSettings.Issuer,
        //            audience: _jwtSettings.Audience,
        //            claims: claims.ToArray(),
        //            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
        //            signingCredentials: creds
        //        );

        //        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //        //4.Lưu session trên server
        //        HttpContext.Session.SetObject<CurrentUser>(_configuration.GetValue<string>("SessionKey"), ObjectMapper.GetCurrentUser(claims.ToDictionary(x => x.Type, x => x.Value)));

        //        return Ok(new
        //        {
        //            access_token = tokenString,
        //            expires = token.ValidTo.AddHours(+7)
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
        //    }
        //}

        [Authorize]
        [RequiresPermission("TEST")]
        //[ApiKeyAuthorize]
        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                var currentUser = ObjectMapper.GetCurrentUser(claims);
                return Ok(ApiResponseFactory.Success(currentUser, ""));

                //string key = _configuration.GetValue<string>("SessionKey") ?? "";
                //CurrentUser currentUser = HttpContext.Session.GetObject<CurrentUser>(key);

                //return Ok(ApiResponseFactory.Success(currentUser, ""));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }
    }
}