using Application.RoadmapActivities;
using FluentValidation;

namespace Application.Dtos
{
    public class UpdateRoadmapValidator : AbstractValidator<UpdateRoadmap.Command>
    {
        public UpdateRoadmapValidator()
        {
            
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .Length(1, 50).WithMessage("Title must be between 1 and 50 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .Length(1, 100).WithMessage("Description must be between 1 and 100 characters.");

            RuleFor(x => x.IsDraft)
                .Must(value => value == false || value == true).WithMessage("IsDraft is required.");

        }
    }
}
