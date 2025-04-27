namespace Tenant;

public interface ITenantService
{
    public string GetConnectionString();
    public string GetDefaultConnectionString();
    public Models.Tenant GetTenant();
}