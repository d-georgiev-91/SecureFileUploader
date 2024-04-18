using FluentValidation;
using SecureFileUploader.Web.Models;

namespace SecureFileUploader.Web.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Username).NotEmpty();
        RuleFor(user => user.Password).NotEmpty();
    }
}