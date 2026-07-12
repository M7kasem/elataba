using Elattba.Core.InterFaces;
using Elattba.Core.Services;
using Elattba.InfraStructure.Data;
using Elattba.InfraStructure.Identity;
using Elattba.InfraStructure.Repository;
using Elattba.InfraStructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elattba.InfraStructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(_ => new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IImageManagementService, ImageManagementService>();
            services.AddSingleton<IImageEmbeddingService, OnnxImageEmbeddingService>();
            services.AddDbContext<El3atbaDbContext>((serviceProvider, op) =>
            {
                op.UseSqlServer(serviceProvider.GetRequiredService<SqlConnection>());
            });

            services.AddDbContext<AppIdentityDbContext>((serviceProvider, op) =>
            {
                op.UseSqlServer(serviceProvider.GetRequiredService<SqlConnection>());
            });

            return services;
        }
    }
}
