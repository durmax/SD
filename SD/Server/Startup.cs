using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using sd.Api.Interfaces;
using sd.Api.Models;
using sd.Api.Repositories;
using sd.Api.Helpers;
using sd.Api.Services;

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
            services.Configure<MongodbSettings>(Configuration.GetSection(nameof(MongodbSettings)));

            services.AddSingleton<IMongodbSettings>(sp =>
                                    sp.GetRequiredService<IOptions<MongodbSettings>>().Value);
            services.AddSingleton<MongodbContext>(x =>
                                    new MongodbContext(x.GetRequiredService<IMongodbSettings>()));

            services.AddTransient<IOtherPageRepository, OtherPageRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IWordRepository, WordRepository>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ILikeWordService, LikeWordService>();
            services.AddTransient<GetRelationshipsService>();
            services.AddTransient<WordService>();

            services.AddDataProtection();

            services.AddControllers();
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

            app.UseCors(builder =>
            {
                builder.WithOrigins("https://localhost:44331",
                    "https://lingoclub.netlify.app",
                    "https://www.lingoclub.net")
                       .WithMethods("GET", "POST", "PUT", "DELETE")
                       .AllowAnyHeader();
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
