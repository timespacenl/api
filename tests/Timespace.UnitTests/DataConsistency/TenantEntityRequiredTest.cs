using System.Reflection;
using System.Runtime.CompilerServices;
using Timespace.Api.Infrastructure;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.UnitTests.DataConsistency;

public class TenantEntityRequiredTest
{
    [Test]
    public void TestTenantEntityRequired()
    {
        var type = typeof(ITenantEntity);
        var types = typeof(IAssemblyMarker).Assembly.GetTypes()
            .Where(p => type.IsAssignableFrom(p));

        List<Type> failTypes = new List<Type>();
        
        foreach (var t in types)
        {
            if (t.GetProperty(nameof(ITenantEntity.TenantId))?.GetCustomAttribute<RequiredMemberAttribute>() == null && !t.FullName!.EndsWith("ITenantEntity"))
            {
                failTypes.Add(t);
            }
        }

        if (failTypes.Count > 0)
        {
            Assert.Fail($"{string.Join(',', failTypes)} tenant guid does not have the RequiredMemberAttribute");
        }
    }
}