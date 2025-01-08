using Application.DTOs;
using FluentValidation;

namespace Application.Dtos
{
    public class RoadmapValidatorDto : AbstractValidator<RoadmapDto>
    {
        public RoadmapValidatorDto()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .Length(1, 50).WithMessage("Title must be between 1 and 50 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .Length(1, 100).WithMessage("Description must be between 1 and 100 characters.");

            RuleFor(x => x.IsDraft)
             .Must(value => value == false || value == true).WithMessage("IsDraft is required.");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("CreatedBy is required.");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.");
        }
    }
}
