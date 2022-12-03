using FluentValidation;

namespace Timespace.Api.Infrastructure.Extensions;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, string> OneOf<T>(this IRuleBuilder<T, string> ruleBuilder, string[] values)
    {
        return ruleBuilder.Must(values.Contains);
    }
}