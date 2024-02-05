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
            //services.AddSingleton<MongodbContext>(x => new MongodbContext(x.GetRequiredService<IMongodbSettings>()));
            services.AddSingleton<MongodbContext>();

            services.AddScoped<IOtherPageRepository, OtherPageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ILikeWordService, LikeWordService>();
            services.AddScoped<IRelationshipRepository, RelationshipRepository>();
            services.AddScoped<WordService>();
            services.AddScoped<RelationshipService>();
            services.AddScoped<UserService>();
            services.AddScoped<OtherPageService>();

            services.AddDataProtection();

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerGen();
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

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        }
    }
}
