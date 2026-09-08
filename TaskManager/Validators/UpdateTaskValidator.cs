using FluentValidation;
using TaskManager.DB.DTO;

namespace TaskManager.Validators
{
    public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskValidator(DateTime createDate)
        {
            RuleFor(task => task.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MinimumLength(4)
                .WithMessage("The title must contain at least 4 characters.");
            RuleFor(x => x.DueDate)
            .Must(dueDate =>
                !dueDate.HasValue ||
                dueDate.Value > createDate)
            .WithMessage("Due date must be later than creation date.");
        }
    }
}
