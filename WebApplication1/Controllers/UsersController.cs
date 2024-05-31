global using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
//using System.Web.Http;


namespace MyMakler.Controllers
{
    /// <summary>
    /// ѕросмотр пользователей и работа с ними
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        public UsersController(ILogics logics)
        {
            _Logics = logics;
        }
        private readonly ILogics _Logics;


        [Authorize]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddUser(ProfileInput profileInput)
        {
            await _Logics.TryAddUserProfile(profileInput);
            return Ok();
        }
        

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> SearchUserProfile(string name)
        {
            var result = await _Logics.TrySearchUserProfile(name);
            if (result == null)
                return NoContent();
            return Ok(result);
        }

        
        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetAllUserProfiles(int page = 1, int pageSize = 10)
        {
            var result = await _Logics.TryGetUserProfilesList(page, pageSize);
            if (result == null)
                return NoContent();
            return Ok(result);
        }


        [HttpGet]
        [Route("PgCount")]
        public async Task<IActionResult> GetUsersPagesCount(int pageSize = 10)
        {
            return Ok(await _Logics.TryGetUserProfilesPagesCount(pageSize));
        }


        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteUser(Guid guid)
        {
            await _Logics.TryDeleteUserProfile(guid);
            return Ok();
        }


        [Authorize]
        [HttpPut]
        [Route("Edit")]
        public async Task<IActionResult> EditUserProfile(ProfileInput profileInput)
        {
            await _Logics.TryEditUserProfile(profileInput);
            return Ok();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            Guid userId = await _Logics.TryLogin(loginModel);
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, loginModel.Login) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.Issuer,
                    audience: AuthOptions.Client,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return Ok(new UserIdAndJwtOutput(new JwtSecurityTokenHandler().WriteToken(jwt), userId));
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {            
            return Ok(await _Logics.TryRegisterUser(model));
        }
        [HttpPatch]
        [Route("DbgInitDb")]
        public async Task<IActionResult> Init()
        {
            await _Logics.InitializeDb();
            return Ok();
        }
    }
}

