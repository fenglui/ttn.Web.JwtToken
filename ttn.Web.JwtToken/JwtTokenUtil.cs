using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ttn.Web.JwtToken
{
    public interface ILoginNameModel
    {
        string username { get; }
    }

    public interface ILoginModel: ILoginNameModel
    {
        string password { get; }
    }

    public static class AuthExtension
    {
        public static async Task<ClaimsIdentity> AuthenticateAsync<TUser>(this SignInManager<TUser> _signinManager, TUser user) where TUser : class
        {
            var principal = await _signinManager.CreateUserPrincipalAsync(user);

            return principal.Identities.First();
        }

        public static async Task<ClaimsIdentity> AuthenticateAsync<TUser>(this SignInManager<TUser> _signinManager, TUser user, string password) where TUser : class
        {
            var passwordSignInResult = await _signinManager.CheckPasswordSignInAsync(user, password, false);

            if (!passwordSignInResult.Succeeded)
            {
                return null;
            }

            var principal = await _signinManager.CreateUserPrincipalAsync(user);

            return principal.Identities.First();

        }
    }

    public class JwtTokenUtil
    {
        private readonly TokenProviderOptions _options;
        private readonly TokenRefreshService _tokenRefreshService;

        public JwtTokenUtil(
            IOptions<TokenProviderOptions> options,
            TokenRefreshService tokenRefreshService)
        {
            _options = options.Value;
            _tokenRefreshService = tokenRefreshService;
        }

        public TokenProviderOptions GetOptions()
        {
            return _options;
        }

        public async Task<TokenResponse> AuthorizeClientAsync<TUser>(UserManager<TUser> _userManager, SignInManager<TUser> _signinManager, string username) where TUser : class
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var identity = await AuthenticateAsync(_userManager, _signinManager, username);

            if (identity == null)
            {
                return null;
            }

            return await IssueJwtAsync(identity);
        }

        public async Task<TokenResponse> AuthorizeClientAsync<TUser>(UserManager<TUser> _userManager, SignInManager<TUser> _signinManager, string username, string password) where TUser : class
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var identity = await AuthenticateAsync(_userManager, _signinManager, username, password);

            if (identity == null)
            {
                return null;
            }

            return await IssueJwtAsync(identity);
        }

        public async Task<TokenResponse> RefreshTokenAsync<TUser>(UserManager<TUser> _userManager, SignInManager<TUser> _signinManager, String refreshToken) where TUser : class
        {
            TokenRefreshServiceResponse tokenResponse = _tokenRefreshService.ValidateRefreshToken(refreshToken);

            if (tokenResponse.Authenticated)
            {
                var user = await _userManager.FindByIdAsync(tokenResponse.UserId.ToString());

                if (user == null)
                    return null;

                var principal = await _signinManager.CreateUserPrincipalAsync(user);

                if (principal != null)
                    return await IssueJwtAsync(principal.Identities.First());
            }

            return null;
        }

        public async Task<TokenResponse> IssueJwtAsync(ClaimsIdentity identity)
        {
            var now = DateTimeOffset.Now;

            TokenResponse response = null;

            await Task.Run(() =>
            {
                string userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                string refreshToken = _tokenRefreshService.GetRefreshToken(userId);
                JwtSecurityToken jwt = GetSecurityToken(identity.Claims);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                response = new TokenResponse
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)_options.Expiration.TotalSeconds,
                    RefreshToken = refreshToken
                };
            });
            // Serialize and return the response
            return response;
        }

        private JwtSecurityToken GetSecurityToken(IEnumerable<Claim> claims)
        {
            //sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey)); // 获取密钥
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // 凭证 ，根据密钥生成
            var now = DateTime.Now;

            return new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_options.Expiration),
                    signingCredentials: creds);
        }

        public string GetToken(string userName)
        {
            // push the user’s name into a claim, so we can identify the user later on.
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                //new Claim(ClaimTypes.Role, admin)//在这可以分配用户角色，比如管理员 、 vip会员 、 普通用户等
            };

            JwtSecurityToken jwt = GetSecurityToken(claims);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string GetToken(ILoginNameModel user)
        {
            return GetToken(user.username);
        }

        private async Task<ClaimsIdentity> AuthenticateAsync<TUser>(UserManager<TUser> _userManager, SignInManager<TUser> _signinManager, string username, string password) where TUser : class
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return null;
            }

            return await _signinManager.AuthenticateAsync(user, password);
        }

        private async Task<ClaimsIdentity> AuthenticateAsync<TUser>(UserManager<TUser> _userManager, SignInManager<TUser> _signinManager, string username) where TUser : class
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return null;
            }

            return await _signinManager.AuthenticateAsync(user);
        }
    }
}
