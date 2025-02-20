using Domain.Dtos;
using FluentValidation;
using System.Text.Json;

namespace Application.Validator
{
    public class CreateLogsValidator : AbstractValidator<RoadmapLogsDto>
    {
        public CreateLogsValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid UserId format.");

            RuleFor(x => x.ActivityAction)
                .NotEmpty().WithMessage("ActivityAction is required.")
                .Must(value => value is string).WithMessage("ActivityAction must be a string.")
                .MinimumLength(5).WithMessage("ActivityAction must be at least 5 characters long.")
                .MaximumLength(100).WithMessage("ActivityAction must not exceed 100 characters.");

            RuleFor(x => x.AdditionalData)
                .Must(additionalData => additionalData == null || additionalData.Count == 0)
                .WithMessage(x => x.AdditionalData != null
                    ? $"Invalid fields detected: {string.Join(", ", x.AdditionalData.Keys)}"
                    : "Invalid fields detected.");
        }
    }
}
