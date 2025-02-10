using FluentValidation;
using Domain.Dtos;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Task Name is required.")
            .Length(1, 50).WithMessage("Task Name must be between 1 and 50 characters.");

        RuleFor(x => x.DateStart)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThanOrEqualTo(x => x.DateEnd).WithMessage("Start date must be before end date.");

        RuleFor(x => x.DateEnd)
            .NotEmpty().WithMessage("End date is required.");
    }
}
