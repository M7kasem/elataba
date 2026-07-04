using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Elattba.InfraStructure.Data
{
    public class El3atbaDbContextFactory : IDesignTimeDbContextFactory<El3atbaDbContext>
    {
        public El3atbaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<El3atbaDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=El3ttbaDb;Trusted_Connection=True;TrustServerCertificate=True");

            return new El3atbaDbContext(optionsBuilder.Options);
        }
    }
}
