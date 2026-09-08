using FluentValidation;
using TaskManager.DB.DTO;

namespace TaskManager.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(user => user.Login)
                .NotEmpty()
                .WithMessage("Login is required.")
                .EmailAddress()
                .WithMessage("Login must be an email address.");
            RuleFor(user => user.UserName)
                .NotEmpty()
                .WithMessage("User name is required")
                .MinimumLength(4)
                .WithMessage("The user name must contain at least 4 characters.");
            RuleFor(user => user.Position)
                .NotEmpty()
                .WithMessage("Position is required");
        }
    }
}
