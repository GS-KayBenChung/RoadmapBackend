using FluentValidation;
using Domain.Dtos;

public class CreateMilestoneValidator : AbstractValidator<CreateMilestoneDto>
{
    public CreateMilestoneValidator(IValidator<CreateSectionDto> sectionValidator) 
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Milestone Name is required.")
            .Length(1, 50).WithMessage("Milestone Name must be between 1 and 50 characters.");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Milestone Description is required if Name is provided.")
                .Length(1, 100).WithMessage("Milestone Description must be between 1 and 50 characters.");
        });

        RuleForEach(x => x.Sections)
            .SetValidator(sectionValidator); 
    }
}
