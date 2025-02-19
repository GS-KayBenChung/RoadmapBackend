using Application.AuditActivities;
using FluentValidation;

namespace Application.Validator
{
    public class GetLogsValidator : AbstractValidator<GetLogs.Query>
    {
        private static readonly string[] AllowedFilters = { "created", "updated", "deleted" };
        private static readonly string[] AllowedSortFields = { "activityaction", "createdat", "name" };

        public GetLogsValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .Must(size => size == 5 || size % 5 == 0)
                .WithMessage("Page size must be 5 or a multiple of 5.")
                .When(x => x.PageSize != 0)
                .LessThanOrEqualTo(20)
                .WithMessage("Page size cannot be more than 20.")
                .NotEqual(0)
                .WithMessage("Page size cannot be 0.");

            RuleFor(x => x.Filter)
                .Must(filter => string.IsNullOrEmpty(filter) || AllowedFilters.Contains(filter.ToLower()))
                .WithMessage($"Filter must be one of: {string.Join(", ", AllowedFilters)}.");

            RuleFor(x => x.SortBy)
                .Must(sort => string.IsNullOrEmpty(sort) || AllowedSortFields.Contains(sort.ToLower()))
                .WithMessage($"Sort field must be one of: {string.Join(", ", AllowedSortFields)}.");

            RuleFor(x => x.Asc)
                .Must(asc => asc == 0 || asc == 1)
                .WithMessage("Sort order must be 0 (descending) or 1 (ascending).");

            RuleFor(x => x.CreatedOn)
                .Must(BeValidDate)
                .WithMessage("Invalid date format.");
        }

        private bool BeValidDate(DateTime? date)
        {
            if (!date.HasValue) return true;
            return date.Value.Kind == DateTimeKind.Utc;
        }
    }

}
