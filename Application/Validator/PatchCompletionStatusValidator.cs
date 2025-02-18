using Application.RoadmapActivities;
using FluentValidation;

namespace Application.Validator
{
    public class PatchCompletionStatusValidator : AbstractValidator<PatchCompletionStatus.Command>
    {
        public PatchCompletionStatusValidator()
        {
            RuleFor(x => x.EntityType)
                .NotEmpty().WithMessage("Entity type is required.")
                .Must(et => new[] { "roadmap", "milestone", "section", "task" }.Contains(et.ToLower()))
                .WithMessage("Invalid entity type. Allowed values: roadmap, milestone, section, task.");

            RuleFor(x => x.UpdateDto.Type)
               .NotEmpty().WithMessage("Entity type is required.")
               .Must(et => new[] { "roadmap", "milestone", "section", "task" }.Contains(et.ToLower()))
               .WithMessage("Invalid entity type. Allowed values: roadmap, milestone, section, task.");

            RuleFor(x => x.UpdateDto)
                .NotNull().WithMessage("Update data cannot be null.")
                .Must(dto => dto.Id != Guid.Empty)
                .WithMessage("Id is required and must be a valid GUID.");

            When(x => x.UpdateDto.Progress.HasValue, () =>
            {
                RuleFor(x => x.UpdateDto.Progress)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Progress must be between 0 and 100.");
            });

            When(x => x.UpdateDto.IsCompleted.HasValue, () =>
            {
                RuleFor(x => x.UpdateDto.IsCompleted)
                    .Must(isCompleted => isCompleted == true || isCompleted == false)
                    .WithMessage("isCompleted must be true or false.");
            });

            When(x => x.EntityType.ToLower() == "task", () =>
            {
                RuleFor(x => x.UpdateDto.Progress)
                    .Empty().WithMessage("Tasks do not support progress.");
            });

            RuleFor(x => x.UpdateDto)
                .NotNull().WithMessage("Update data cannot be null.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.UpdateDto.AdditionalData)
                        .Must(additionalData => additionalData == null || additionalData.Count == 0)
                        .WithMessage(x => x.UpdateDto.AdditionalData != null
                            ? $"Invalid fields detected: {string.Join(", ", x.UpdateDto.AdditionalData.Keys)}"
                            : "Invalid fields detected.");
                });


        }
    }
}
