using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tenant.Models;
using Zeka.Extensions.MultiTenant;

namespace Tenant;

public static class MultiTenantExtensions
{
    public static IServiceCollection AddMultiTenantDbContext<TDbContext>(this IServiceCollection services, IConfiguration configuration) where TDbContext : DbContext
    {
        services.Configure<TenantSettings>(configuration.GetSection(nameof(TenantSettings)));

        services.AddHttpContextAccessor();

        var tenantSettingsOptions = services.GetOptions<TenantSettings>(nameof(TenantSettings));

        services.AddTransient<ITenantService, TenantService>();

        var defaultConnectionString = tenantSettingsOptions.DefaultConnectionString;

        services.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(b => b.MigrationsAssembly(typeof(TDbContext).Assembly.FullName)));


        // to revert to the pre-6.0 behavior to avoid the timeZone mapping
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddHealthChecks()
            .AddDbContextCheck<TDbContext>();

        var tenants = tenantSettingsOptions.Tenants;
        foreach (var tenant in tenants)
        {
            string connectionString;
            if (string.IsNullOrEmpty(tenant.ConnectionString))
            {
                connectionString = defaultConnectionString;
            }
            else
            {
                connectionString = tenant.ConnectionString;
            }
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            dbContext.Database.SetConnectionString(connectionString);
            if (dbContext.Database.GetMigrations().Count() > 0)
            {
                dbContext.Database.Migrate();
                //IDbSeeder<DbContext>().Seed(dbContext);
            }
        }

        return services;
    }

    public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
    {
        using var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);
        return options;
    }
}