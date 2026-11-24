using FluentValidation;
using HotelMS.Auth.DTOs;
using HotelMS.Auth.Models.Enums;

namespace HotelMS.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty().Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        RuleFor(x => x.Role).IsEnumName(typeof(UserRole), caseSensitive: false)
            .WithMessage("Role must be Owner, Manager, or Receptionist.");
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Password), () =>
        {
            RuleFor(x => x.Password!).MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        });

        When(x => !string.IsNullOrEmpty(x.Role), () =>
        {
            RuleFor(x => x.Role!).IsEnumName(typeof(UserRole), caseSensitive: false)
                .WithMessage("Role must be Owner, Manager, or Receptionist.");
        });
    }
}
