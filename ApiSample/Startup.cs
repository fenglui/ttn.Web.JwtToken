using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ttn.Web.JwtToken;

namespace ApiSample
{
    public class ApiSampleUser : IdentityUser
    {
        public String TestProperty { get; set; }
    }

    public class DataContext: IdentityDbContext<ApiSampleUser>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=aspnet-ApiSample-BB34FB6C-23D2-4B22-B257-89424DA4F852;Trusted_Connection=True;ConnectRetryCount=0");
            }
        }
    }

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
            services.AddControllers();

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
                        "http://yourclientapp.com");
                }
               );
            });

            services.AddMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;
                }
            )
            //  you need add Microsoft.AspNetCore.Mvc.NewtonsoftJson to your project
            //.AddNewtonsoftJson(options => { options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss"; })
            .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DataContext>(options =>
                {});

            services.AddIdentity<ApiSampleUser, IdentityRole>()
                 .AddEntityFrameworkStores<DataContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.密码配置
                options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 1;

                // Lockout settings.锁定设置
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.用户设置
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
            });

            services.ConfigureJwtTokenServices(
                secretString: "[]197897shunzimm",
                authenticationScheme: Consts.AuthenticationScheme,
                // 7 天
                ExpirationInSeconds: 7 * 24 * 60 * 60
            );

            services.RegisterKContextService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseStaticFiles();
        }
    }

    public static class KContext
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static IServiceCollection RegisterKContextService(this IServiceCollection services,
ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            KContext.ServiceProvider = services.BuildServiceProvider();
            return services;
        }

        public static DataContext NewDataContext()
        {
            return new DataContext(
                ServiceProvider.GetRequiredService<
                    DbContextOptions<DataContext>>());
        }

    }
}
