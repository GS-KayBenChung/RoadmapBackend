using FluentValidation;
using MediatR;
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators; 
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }
       

        var context = new ValidationContext<TRequest>(request);
        var validationFailures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        _logger.LogInformation("Validating request: {@Request}", request);

        if (validationFailures.Any())
        {
            var errors = string.Join(", ", validationFailures.Select(e => e.ErrorMessage));
            _logger.LogWarning("Validation Failed: {Errors}", errors);
            throw new ValidationException(errors);
        }
        _logger.LogInformation("Validating request: {@Request}", request);
        return await next();
    }
}
