//using FluentValidation;
//using Microsoft.Extensions.DependencyInjection;
//using Serilog;

//namespace Application.Validator
//{
//    public interface IValidationService
//    {
//        Task ValidateAsync<T>(T dto, CancellationToken cancellationToken);
//    }

//    public class ValidationService : IValidationService
//    {
//        private readonly IServiceProvider _serviceProvider;

//        public ValidationService(IServiceProvider serviceProvider)
//        {
//            _serviceProvider = serviceProvider;
//        }

//        public async Task ValidateAsync<T>(T dto, CancellationToken cancellationToken)
//        {
//            var validator = _serviceProvider.GetService<IValidator<T>>();
//            if (validator == null)
//            {
//                throw new InvalidOperationException($"No validator found for type {typeof(T).Name}");
//            }

//            var validationResult = await validator.ValidateAsync(dto, cancellationToken);
//            if (!validationResult.IsValid)
//            {
//                var errors = validationResult.Errors
//                    .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

//                Log.Warning("Validation failed: {Errors}", string.Join(", ", errors.Select(e => $"{e.Key}: {e.Value}")));
//                throw new ValidationException(validationResult.Errors);
//            }
//        }
//    }
//}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Validator
{
    public interface IValidationService
    {
        Task ValidateAsync<T>(T dto, CancellationToken cancellationToken);
    }

    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ValidateAsync<T>(T dto, CancellationToken cancellationToken)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator == null)
            {
                throw new InvalidOperationException($"No validator found for type {typeof(T).Name}");
            }

            var validationResult = await validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

                Log.Warning("Validation failed: {Errors}", string.Join(", ", errors.Select(e => $"{e.Key}: {e.Value}")));
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
