using MediatR;

namespace Timespace.Api.Application.Common.Behaviours;

public static class BehaviourServiceCollectionExtensions
{
    public static void RegisterBehaviours(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    }
}