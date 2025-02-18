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

        RuleFor(x => x)
           .NotNull().WithMessage("Roadmap data cannot be null.")
           .DependentRules(() =>
           {
               RuleFor(x => x.AdditionalData)
                   .Must(additionalData => additionalData == null || additionalData.Count == 0)
                   .WithMessage(x => x.AdditionalData != null
                       ? $"Invalid fields detected: {string.Join(", ", x.AdditionalData.Keys)}"
                       : "Invalid fields detected.");
           });
    }
    public class CreateMilestoneValidator : AbstractValidator<CreateMilestoneDto>
    {
        public CreateMilestoneValidator(IValidator<CreateSectionDto> sectionValidator)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Milestone name is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Milestone description is required.");

            RuleForEach(x => x.Sections)
                .SetValidator(sectionValidator);

            RuleFor(x => x.AdditionalData)
                .Must(additionalData => additionalData == null || additionalData.Count == 0)
                .WithMessage(x => x.AdditionalData != null
                    ? $"Invalid fields detected: {string.Join(", ", x.AdditionalData.Keys)}"
                    : "Invalid fields detected.");
        }
    }

    public class CreateSectionValidator : AbstractValidator<CreateSectionDto>
    {
        public CreateSectionValidator(IValidator<CreateTaskDto> taskValidator)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Section name is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Section description is required.");

            RuleForEach(x => x.Tasks)
                .SetValidator(taskValidator);

            RuleFor(x => x.AdditionalData)
                .Must(additionalData => additionalData == null || additionalData.Count == 0)
                .WithMessage(x => x.AdditionalData != null
                    ? $"Invalid fields detected: {string.Join(", ", x.AdditionalData.Keys)}"
                    : "Invalid fields detected.");
        }
    }

    public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Task name is required.");

            RuleFor(x => x.DateStart)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.DateEnd)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.DateStart).WithMessage("End date must be after start date.");

            RuleFor(x => x.AdditionalData)
                .Must(additionalData => additionalData == null || additionalData.Count == 0)
                .WithMessage(x => x.AdditionalData != null
                    ? $"Invalid fields detected: {string.Join(", ", x.AdditionalData.Keys)}"
                    : "Invalid fields detected.");
        }
    }
}
