using FluentValidation;

namespace TaskStream.Tasks.Application.Validators;

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();

        When(dto => dto.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название задачи не может быть пустым.")
                .MaximumLength(100).WithMessage("Длина названия задачи не должна превышать 100 символов.");
        });

        When(dto => dto.Description is not null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(500).WithMessage("Длина описания задачи не должна превышать 500 символов.");
        });

        RuleFor(dto => dto)
            .Must(dto =>
                dto.Title is not null ||
                dto.Description is not null ||
                dto.Status.HasValue ||
                dto.DueDate.HasValue)
            .WithMessage("Для обновления задачи необходимо предоставить хотя бы одно поле.");
    }
}