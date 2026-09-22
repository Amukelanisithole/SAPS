using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database.AppDBContext
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=.;Database=SAPS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
