using FluentValidation;
using TaskManager.DB.DTO;

namespace TaskManager.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(user => user.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(8)
                .WithMessage("The password must contain at least 8 characters.");
        }
    }
}
