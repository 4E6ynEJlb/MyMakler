global using LogicsLib;
global using Repos;
using LogicsLib.Services;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using MyMakler.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

namespace MyMakler
{
    public class Program
    {
        public static void Main(string[] args)
        {                
            var builder = WebApplication.CreateBuilder();            
            builder.Services.AddDbContext<ApplicationContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Repos")); options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution); });
            builder.Services.AddScoped<ILogics, Logics>();
            // Add services to the container.
            builder.Services.AddHostedService<DetachedPicsService>();
            builder.Services.AddHostedService<OldAdsService>();
            builder.Services.AddImageResizeMW();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.ExampleFilters();
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = @"Введите JWT токен авторизации.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                        },
                        new List<string>()
                    }
                });
            });
            builder.Services.AddSwaggerExamplesFromAssemblyOf<GetAllAdsArgsExample>();
            builder.Services.Configure<ConstsOptions>(builder.Configuration.GetSection(ConstsOptions.ConstsConfiguration));
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.Client,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                        
                    };
                    options.SaveToken = true;
                }
                );
            DirectoryInfo drinfo = new DirectoryInfo(builder.Configuration.GetSection(ConstsOptions.ConstsConfiguration).GetValue<string>("PicsDirectory"));
            if (!drinfo.Exists)
            {
                drinfo.Create();
            }
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.Environment.WebRootPath = builder.Configuration.GetSection(ConstsOptions.ConstsConfiguration).GetValue<string>("PicsDirectory");
            app.UseResizeMiddleware();
            app.UseFileServer(new FileServerOptions      
            {
                EnableDirectoryBrowsing = true,
                FileProvider = new PhysicalFileProvider(builder.Configuration.GetSection(ConstsOptions.ConstsConfiguration).GetValue<string>("PicsDirectory")),
                RequestPath = new PathString("/pics"),
                EnableDefaultFiles = false,
                
            });
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();                
            }
            app.UseExceptionHandlerMiddleware();
            app.UseHttpsRedirection();

            
            
            app.MapControllers();
            app.Run();
        }

    }
}
