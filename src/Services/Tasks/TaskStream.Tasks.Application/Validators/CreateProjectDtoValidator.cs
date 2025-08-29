using FluentValidation;
using TaskStream.Tasks.Application.DTOs;

namespace TaskStream.Tasks.Application.Validators;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название проекта не может быть пустым.")
            .MaximumLength(100).WithMessage("Длина названия проекта не должна превышать 100 символов.");
    }
}