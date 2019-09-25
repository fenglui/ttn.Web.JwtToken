using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ttn.Web.JwtToken
{
    /// <summary>
    /// 注册JwtToken服务
    /// </summary>
    public static class Register
    {
        public static void ConfigureJwtTokenServices(this IServiceCollection services, string authenticationScheme = "Bearer", string audience = "api", string issuer = "api", string secretString = "ttn.Web.JwtToken.secretString", int ExpirationInSeconds = 30 * 60)
        {
            services.Configure<TokenProviderOptions>(opt =>
            {
                opt.Audience = audience;
                opt.Issuer = issuer;
                opt.SecurityKey = secretString;
                opt.Expiration = TimeSpan.FromSeconds(ExpirationInSeconds);
            });

            // Services used by identity
            services.AddAuthentication(options =>
            {
                // options.DefaultScheme = authenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(opt =>
            {

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretString));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    // 是否验证SecurityKey
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = secretKey,
                    // 是否验证Issuer
                    ValidateIssuer = true, 
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    // 是否验证超时  当设置exp和nbf时有效 同时启用ClockSkew 
                    ValidateLifetime = true,
                    // 注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
                    ClockSkew = TimeSpan.Zero,
                };
                opt.TokenValidationParameters = tokenValidationParameters;
                opt.SaveToken = true;
            });

            services.AddSingleton<TokenRefreshService>();
            services.AddSingleton<JwtTokenUtil>();
        }
    }
}
