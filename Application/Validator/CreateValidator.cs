using FluentValidation;

namespace Application.Validator
{
    public class CreateValidator : AbstractValidator<Create.Command>
    {
        public CreateValidator()
        {
            RuleFor(x => x.RoadmapDto)
                .NotNull().WithMessage("RoadmapDto is required.");

            RuleFor(x => x.RoadmapDto.Title)
                .NotEmpty().WithMessage("Title is required.")
                .When(x => x.RoadmapDto.Title != null)
                .Length(1, 50).WithMessage("Title must be between 1 and 50 characters.");

            RuleFor(x => x.RoadmapDto.Description)
                .NotEmpty().WithMessage("Description is required.")
                .When(x => x.RoadmapDto.Description != null)
                .Length(1, 100).WithMessage("Description must be between 1 and 100 characters.");

            RuleFor(x => x.RoadmapDto.CreatedBy)
                .NotEqual(Guid.Empty).WithMessage("CreatedBy is required.");



            RuleFor(x => x.RoadmapDto.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CreatedAt cannot be in the future.");
        }
    }
}
