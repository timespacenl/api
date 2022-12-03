namespace Timespace.Api.Application.Features.Tenants.Common.Entities;

public partial class Tenant
{
    public string CompanyName { get; set; } = null!;
    public string CompanyIndustry { get; set; } = null!;
    public int CompanySize { get; set; } 
}