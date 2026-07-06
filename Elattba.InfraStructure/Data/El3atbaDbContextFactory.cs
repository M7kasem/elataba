using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Elattba.InfraStructure.Data
{
    public class El3atbaDbContextFactory : IDesignTimeDbContextFactory<El3atbaDbContext>
    {
        public El3atbaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<El3atbaDbContext>();
            var connectionString = GetConnectionString();

            optionsBuilder.UseSqlServer(connectionString);

            return new El3atbaDbContext(optionsBuilder.Options);
        }

        private static string GetConnectionString()
        {
            var environmentConnectionString =
                Environment.GetEnvironmentVariable("EL3TTBA_CONNECTION_STRING") ??
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (!string.IsNullOrWhiteSpace(environmentConnectionString))
            {
                return environmentConnectionString;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var apiProjectDirectory = Path.GetFullPath(Path.Combine(
                currentDirectory,
                "..",
                "Elattaba.API"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.Exists(apiProjectDirectory) ? apiProjectDirectory : currentDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found. Set EL3TTBA_CONNECTION_STRING or ConnectionStrings__DefaultConnection.");
        }
    }
}
