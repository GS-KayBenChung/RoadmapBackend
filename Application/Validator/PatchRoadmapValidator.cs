using Application.RoadmapActivities;
using FluentValidation;

namespace Application.Validator
{
    public class PatchRoadmapValidator : AbstractValidator<PatchRoadmap.Command>
    {
        public PatchRoadmapValidator()
        {
            RuleFor(x => x.RoadmapId)
                .NotEmpty().WithMessage("Roadmap ID is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("Invalid Roadmap ID format.");

            RuleFor(x => x.UpdateDto)
                .NotNull().WithMessage("Update data cannot be null.");

            When(x => x.UpdateDto != null, () =>
            {
                RuleFor(x => x.UpdateDto.Roadmap)
                    .Must(roadmap => roadmap == null || !string.IsNullOrWhiteSpace(roadmap.Title) || !string.IsNullOrWhiteSpace(roadmap.Description))
                    .WithMessage("If provided, Roadmap title or description cannot be empty.");

                RuleFor(x => x.UpdateDto.Roadmap.Title)
                   .NotEmpty().When(x => x.UpdateDto.Roadmap != null)
                   .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.UpdateDto.Roadmap.Title))
                   .WithMessage("Title must not exceed 50 characters.");

                RuleFor(x => x.UpdateDto.Roadmap.Description)
                    .NotEmpty().When(x => x.UpdateDto.Roadmap != null)
                    .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.UpdateDto.Roadmap.Description))
                    .WithMessage("Description must not exceed 100 characters.");

                RuleForEach(x => x.UpdateDto.Milestones)
                    .Must(m => m.MilestoneId != Guid.Empty)
                    .WithMessage("Each milestone must have a valid milestoneId.");

                RuleForEach(x => x.UpdateDto.Sections)
                    .Must(s => s.SectionId != Guid.Empty && s.MilestoneId != Guid.Empty)
                    .WithMessage("Each section must have a valid SectionId and MilestoneId.");

                RuleForEach(x => x.UpdateDto.Tasks)
                    .Must(t => t.TaskId != Guid.Empty && t.SectionId != Guid.Empty && t.MilestoneId != Guid.Empty)
                    .WithMessage("Each task must have a valid TaskId, SectionId, and MilestoneId.")
                    .Must(t => t.DateStart == null || t.DateEnd == null || t.DateStart < t.DateEnd)
                    .WithMessage("Task start date must be before the end date.");

                RuleFor(x => x.UpdateDto)
                    .Must(dto => dto.AdditionalData == null || dto.AdditionalData.Count == 0)
                    .WithMessage(x => x.UpdateDto.AdditionalData != null
                        ? $"Invalid fields detected: {string.Join(", ", x.UpdateDto.AdditionalData.Keys)}"
                        : "Invalid fields detected.");

                RuleFor(x => x.UpdateDto.Roadmap)
                    .Must(dto => dto == null || dto.AdditionalData == null || dto.AdditionalData.Count == 0)
                    .WithMessage(x => x.UpdateDto.Roadmap.AdditionalData != null
                        ? $"Invalid fields detected in Roadmap: {string.Join(", ", x.UpdateDto.Roadmap.AdditionalData.Keys)}"
                        : "Invalid fields detected in Roadmap.");

                RuleForEach(x => x.UpdateDto.Milestones)
                    .Must(m => m.AdditionalData == null || m.AdditionalData.Count == 0)
                    .WithMessage((command, m) => m.AdditionalData != null
                        ? $"Invalid fields detected in Milestone '{m.Name}': {string.Join(", ", m.AdditionalData.Keys)}"
                        : "Invalid fields detected in Milestone.");

                RuleForEach(x => x.UpdateDto.Sections)
                    .Must(s => s.AdditionalData == null || s.AdditionalData.Count == 0)
                    .WithMessage((command, s) => s.AdditionalData != null
                        ? $"Invalid fields detected in Section '{s.Name}': {string.Join(", ", s.AdditionalData.Keys)}"
                        : "Invalid fields detected in Section.");

                RuleForEach(x => x.UpdateDto.Tasks)
                    .Must(t => t.AdditionalData == null || t.AdditionalData.Count == 0)
                    .WithMessage((command, t) => t.AdditionalData != null
                        ? $"Invalid fields detected in Task '{t.Name}': {string.Join(", ", t.AdditionalData.Keys)}"
                        : "Invalid fields detected in Task.");
            });
        }
    }
}
