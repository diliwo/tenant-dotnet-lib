using Microsoft.EntityFrameworkCore;

namespace Zeka.Extensions.MultiTenant;

public interface IDbSeeder<TContext> where TContext : DbContext
{
    static abstract void SeedData(TContext context);
}