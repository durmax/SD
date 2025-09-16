using Microsoft.Extensions.DependencyInjection;
using sd.Application.Services;
using sd.Application.Helper;
using sd.Application.Services.Gemini;

namespace sd.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRelationshipService, RelationshipService>();
            services.AddScoped<IOtherPageService, OtherPageService>();
            services.AddScoped<IWordService, WordService>();
            services.AddSingleton<GeminiService>();
            services.AddSingleton(typeof(CachingHelper));

            return services;
        }
    }
}
