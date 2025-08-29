using FluentValidation;

namespace TaskStream.Tasks.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задачи не может быть пустым.")
            .MaximumLength(100).WithMessage("Длина названия задачи не должна превышать 100 символов.");
    }
}