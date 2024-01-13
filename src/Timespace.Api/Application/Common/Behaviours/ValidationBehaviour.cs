using System.Reflection;
using FluentValidation;
using MediatR;
using Timespace.Api.Application.Common.Attributes;
using ValidationException = Timespace.Api.Application.Common.Exceptions.ValidationException;

namespace Timespace.Api.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        if (request.GetType().GetCustomAttribute<SkipValidationAttribute>() != null)
        {
            return await next();
        }
        
        var context = new ValidationContext<TRequest>(request);
        var errorsDictionary = _validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .GroupBy(
                x => x.PropertyName,
                x => x.ErrorMessage,
                (propertyName, errorMessages) => new
                {
                    Key = propertyName,
                    Values = errorMessages.Distinct().ToArray()
                })
            .ToDictionary(x => x.Key, x => x.Values);
        
        if (errorsDictionary.Any())
        {
            throw new ValidationException(errorsDictionary);
        }
        return await next();
    }
}