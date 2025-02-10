using FluentValidation;
using Domain.Dtos;

public class CreateSectionValidator : AbstractValidator<CreateSectionDto>
{
    public CreateSectionValidator(IValidator<CreateTaskDto> taskValidator) 
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Section Name is required.")
            .Length(1, 50).WithMessage("Section Name must be between 1 and 50 characters.");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Section Description is required if Name is provided.")
                .Length(1, 50).WithMessage("Section Description must be between 1 and 50 characters.");
        });

        RuleForEach(x => x.Tasks)
            .SetValidator(taskValidator); 
    }
}
