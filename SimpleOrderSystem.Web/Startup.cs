using System;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SimpleOrderSystem.Data.Services;
using SimpleOrderSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SimpleOrderSystem.Web.Middleware;
using SimpleOrderSystem.Web.Security;

namespace SimpleOrderSystem.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<DbInitializer>();
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddScoped<CatalogService>();
            services.AddScoped<StockService>();
            services.AddScoped<OrderService>();

            var jwtHelper = new JwtHelper();
            services.AddSingleton(jwtHelper);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "SimpleOrderSystem",
                        ValidAudience = "SimpleOrderSystem",
                        IssuerSigningKey = jwtHelper.SecurityKey,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var value = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                            if (!int.TryParse(value, out var id))
                            {
                                context.Fail("Invalid account.");
                                return;
                            }
                            var service = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                            var user = await service.GetActiveUserAsync(id);
                            if (user == null)
                            {
                                context.Fail("The account is unavailable.");
                                return;
                            }
                            var identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
                            foreach (var role in user.Roles)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                            foreach (var permission in user.Permissions)
                            {
                                identity.AddClaim(new Claim("permission", permission));
                            }
                        },
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsJsonAsync(new { message = "Please sign in." });
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            return context.Response.WriteAsJsonAsync(new { message = "You do not have permission for this operation." });
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                foreach (var permission in new[] { "Users", "Roles", "Customers", "Products", "CreateOrders", "Reports" })
                {
                    options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().RequireClaim("permission", permission));
                }
                options.AddPolicy("ManageStock", policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));
            });
            services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseMiddleware<ErrorMiddleware>();
            if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
