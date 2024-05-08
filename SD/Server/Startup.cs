using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using sd.Api.Interfaces;
using sd.Api.Models;
using sd.Api.Repositories;
using sd.Api.Services;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using sd.Api.Helpers;

namespace sd.Api
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

            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.NameClaimType = "name";
                });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            services.Configure<MongodbSettings>(Configuration.GetSection(nameof(MongodbSettings)));

            services.AddSingleton<IMongodbSettings>(sp =>
                                    sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

            services.AddSingleton<MongodbContext>();

            services.AddScoped<CurrUsrService>();

            services.AddSingleton<IOtherPageRepository, OtherPageRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IWordRepository, WordRepository>();
            services.AddSingleton<ICommentService, CommentService>();
            services.AddSingleton<ILikeWordService, LikeWordService>();
            services.AddSingleton<IRelationshipRepository, RelationshipRepository>();
            services.AddSingleton<WordService>();
            services.AddSingleton<RelationshipService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<OtherPageService>();

            services.AddDataProtection();

            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            services.AddAutoMapper(typeof(Startup));

            // Swagger
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors(builder =>
            {
                builder.WithOrigins(
                    "https://localhost:51473",
                    "https://lingoclub.netlify.app",
                    "https://www.lingoclub.net")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Swagger
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        }
    }
}
