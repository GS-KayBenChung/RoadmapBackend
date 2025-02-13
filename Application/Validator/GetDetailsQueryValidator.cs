using FluentValidation;
using Application.RoadmapActivities;

namespace Application.Validator
{
    public class GetDetailsQueryValidator : AbstractValidator<GetDetails.Query>
    {
        public GetDetailsQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Roadmap ID is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("Invalid Roadmap ID format.");
        }
    }
}
