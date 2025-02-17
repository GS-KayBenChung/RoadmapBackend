using FluentValidation;
using Application.RoadmapActivities;

namespace Application.Validator
{
    public class DeleteRoadmapValidator : AbstractValidator<Delete.Command>
    {
        public DeleteRoadmapValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Roadmap ID is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("Invalid Roadmap ID format.");
        }
    }
}
