using Master.Authentication.JwtBearer;
using Master.Models.TokenAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Master.MultiTenancy;
using Master.Authentication;
using Abp.Domain.Entities;

namespace Master.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : MasterControllerBase
    {
        private readonly LoginResultTypeHelper _loginResultTypeHelper;
        private readonly LoginManager _loginManager;
        private readonly UserManager _userManager;
        private readonly TokenAuthConfiguration _configuration;
        public TokenAuthController(
            TokenAuthConfiguration configuration,
            LoginManager loginManager,
            LoginResultTypeHelper loginResultTypeHelper,
            UserManager userManager
            )
        {
            _configuration = configuration;
            _loginManager = loginManager;
            _loginResultTypeHelper = loginResultTypeHelper;
            _userManager = userManager;
        }
        
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<AuthenticateResultModel> Authenticate(AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserName,
                model.Password,
                model.TenancyName
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            var authenticateModel= new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncrpyedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };

            var user = await _userManager.GetByIdAsync(authenticateModel.UserId);
            user.SetData("currentToken", authenticateModel.EncryptedAccessToken);
            await _userManager.UpdateAsync(user);

            return authenticateModel;
        }
        private async Task<LoginResult> GetLoginResultAsync(string username, string password, string tenancyName)
        {
            var loginResult = await _loginManager.LoginAsync(username, password, tenancyName);

            switch (loginResult.Result)
            {
                case LoginResultType.Success:
                    return loginResult;
                default:
                    throw _loginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, username, tenancyName);
            }
        }
        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }
        private string GetEncrpyedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
        
    }
}
