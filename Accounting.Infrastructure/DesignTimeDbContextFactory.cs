using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accounting.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
    {
        public AccountingDbContext CreateDbContext(string[] args)
        {
            var cs = "Server=.\\SQLEXPRESS04;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseSqlServer(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory", AccountingDbContext.Schema))
                .Options;

            return new AccountingDbContext(options);
        }
    }
}
