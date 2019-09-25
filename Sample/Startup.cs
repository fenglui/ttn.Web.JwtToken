using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using ttn.Web.JwtToken;
using Microsoft.Extensions.Hosting;

namespace ttn.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins(
                        "http://localhost:9528",
                        "http://www.yourdomain.com");
                }
               );
            });

            services.AddResponseCaching();
            services.AddMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    //options.CacheProfiles.Add("default", new Microsoft.AspNetCore.Mvc.CacheProfile
                    //{
                    //    Duration = 600,  // 10 min
                    //});

                    //options.CacheProfiles.Add("Hourly", new Microsoft.AspNetCore.Mvc.CacheProfile
                    //{
                    //    Duration = 60 * 60,  // 1 hour
                    //                         //Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                    //                         //NoStore = true,
                    //                         //VaryByHeader = "User-Agent",
                    //                         //VaryByQueryKeys = new string[] { "aaa" }
                    //});
                }
            )
            //.AddJsonOptions(options =>
            //{
            //    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            //})
            //  ref Microsoft.AspNetCore.Mvc.NewtonsoftJson
            .AddNewtonsoftJson(options => { options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"; })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSession();


            // Identity Options
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(7);
                //options.LoginPath = $"/Identity/Account/Login";
                //options.LogoutPath = $"/Identity/Account/Logout";
                //options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.ConfigureJwtTokenServices(
                secretString: "yoursecret_here",
                authenticationScheme: Consts.AuthenticationScheme,
                // 7 天
                ExpirationInSeconds: 7 * 24 * 60 * 60
            );

            // KContext.ServiceProvider = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCaching();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        Public = true,
                        //for 1 year
                        MaxAge = System.TimeSpan.FromDays(365)
                    };
                }
            });
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc(
            //routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //}
            );
        }
    }

    /// <summary>
    /// http://www.sohu.com/a/260462797_100016227
    /// 在程序的任意位置使用和注入服务
    /// </summary>
    public static class KContext
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }
}
