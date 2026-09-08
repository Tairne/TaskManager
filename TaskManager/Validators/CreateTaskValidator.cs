using FluentValidation;
using TaskManager.DB.DTO;

namespace TaskManager.Validators
{
    public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskValidator()
        {
            RuleFor(task => task.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MinimumLength(4)
                .WithMessage("The title must contain at least 4 characters.");
        }
    }
}
