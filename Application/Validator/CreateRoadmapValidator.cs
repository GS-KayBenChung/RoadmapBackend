using Domain.Dtos;
using FluentValidation;

public class CreateRoadmapValidator : AbstractValidator<CreateRoadmapDto>
{
    public CreateRoadmapValidator(IValidator<CreateMilestoneDto> milestoneValidator)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 100 characters.");

        RuleFor(x => x.IsDraft)
            .NotNull().WithMessage("IsDraft is required."); 

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required.");

        RuleFor(x => x.CreatedAt)
            .NotNull().WithMessage("CreatedAt is required."); 

        RuleFor(x => x.Milestones)
            .NotNull().When(x => x.IsDraft == false)
            .WithMessage("Milestones are required when IsDraft is false.");

        RuleFor(x => x.Milestones)
            .NotEmpty().When(x => x.IsDraft == false)
            .WithMessage("At least one milestone is required when IsDraft is false.");

        RuleForEach(x => x.Milestones)
            .SetValidator(milestoneValidator);
    }
}
