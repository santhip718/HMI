using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VmcHmi.Infrastructure;

public class HmiDbContextFactory : IDesignTimeDbContextFactory<HmiDbContext>
{
    public HmiDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HMI_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=learning_portal;Username=postgres;Password=postgres;SearchPath=hmi";

        var optionsBuilder = new DbContextOptionsBuilder<HmiDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.MigrationsHistoryTable("_hmi_migrations", "hmi"));

        return new HmiDbContext(optionsBuilder.Options);
    }
}
