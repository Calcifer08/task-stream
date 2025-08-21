using FluentValidation;
using TaskStream.Auth.Application.DTOs;

namespace TaskStream.Auth.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым.")
            .EmailAddress().WithMessage("Указан некорректный формат Email.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым.")
            .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов.")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
            .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
            .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру.");
    }
}