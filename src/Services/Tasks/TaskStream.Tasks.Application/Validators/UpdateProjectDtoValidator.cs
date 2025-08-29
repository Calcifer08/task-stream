using FluentValidation;
using TaskStream.Tasks.Application.DTOs;

namespace TaskStream.Tasks.Application.Validators;

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();

        When(dto => dto.NewTitle is not null, () =>
        {
            RuleFor(dto => dto.NewTitle)
                .NotEmpty().WithMessage("Новое название проекта не может быть пустым.")
                .MaximumLength(100).WithMessage("Длина названия проекта не должна превышать 100 символов.");
        });

        When(dto => dto.NewDescription is not null, () =>
        {
            RuleFor(dto => dto.NewDescription!)
               .MaximumLength(500).WithMessage("Длина описания проекта не должна превышать 500 символов.");
        });

        RuleFor(dto => dto)
            .Must(dto => dto.NewTitle is not null || dto.NewDescription is not null)
            .WithMessage("Для обновления проекта необходимо предоставить хотя бы одно поле.");
    }
}