namespace Zeka.Extensions.MultiTenant;

public interface IHasTenant
{
    public string TenantName { get; set; }
}