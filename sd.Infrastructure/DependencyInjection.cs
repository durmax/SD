using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using sd.Infrastructure.Models;
using sd.Infrastructure.Repositories;
using sd.Application.Interfaces.Repositories;

namespace sd.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {

            services.AddSingleton<IMongodbSettings>(sp =>
                                    sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

            services.AddSingleton<MongodbContext>();

            services.AddScoped<IOtherPageRepository, OtherPageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<IRelationshipRepository, RelationshipRepository>();
            services.AddScoped<IUserConfigRepository, UserConfigRepository>();

            return services;
        }
    }
}
